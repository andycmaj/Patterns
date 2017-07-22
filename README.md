# Patterns Library

## Commands

Single-responsibility, idempotent units of business logic with clear dependency contracts and easy testability.

Commands that return a result are **Functions** and those that don't are **Actions**.

### Basic Example

#### Step 1: Define your Command and result types

Commands are basically _POCOs_ that encapsulate all input needed to execute your business logic. Commands that implement `IFunction<TResult>` return a result of type `TResult`.

:muscle: Best Practice: Organize your Command's associated classes (results and handlers) as inner classes. This convention makes it easy to find and predict all the various classes.

```csharp
public class AuthenticateUserCommand : IFunction<AuthenticateUserCommand.Result>
{
    public int UserId { get; private set; }
    public string Password { get; private set; }

    public AuthenticateUserCommand(int userId, string password)
    {
        UserId = userid;
        Password = password;
    }

    public class Result
    {
        public bool IsAuthenticated { get; set; }
    }

    ...
}
```

#### Step 2: Define your CommandHandler

The business logic is implemented in the Command's Handler, which implements `IFunctionHandler<TCommand, TResult>`. Any dependencies the Handler needs must be injected into its constructor.

```csharp
public class AuthenticateUserCommand : IFunction<AuthenticateUserCommand.Result>
{
    ...

    public class Handler : IFunctionHandler<AuthenticateUserCommand, AuthenticateUserCommand.Result>
    {
        private readonly IUserService userService;

        public Handler(IUserService userService)
        {
            this.userService = userService;
        }

        public AuthenticateUserCommand.Result Execute(AuthenticateUserCommand command)
        {
            var isAuthenticated =
                userService.VerifyPassword(
                    command.UserId,
                    command.Password
                );

            return new Result {
                IsAuthenticated = isAuthenticated
            };
        }
    }
}

```

#### Step 3: Execute your Command

```csharp
var command = new AuthenticateUserCommand(42, "P@ssw0rd");
var handler = new AuthenticateUserCommand.Handler(mockUserService);

var result = handler.Execute(command);

Assert.IsTrue(result.IsAuthenticated);
```

### Actions

Commands that do not need to return a result should implement `IAction`.

```csharp
public class ResetPasswordCommand : IAction
{
    public int UserId { get; set; }

    public class Handler : IActionHandler<ResetPasswordCommand>
    {
        private readonly IUserService userService;

        public Handler(IUserService userService)
        {
            this.userService = userService;
        }

        public void Execute(ResetPasswordAction action)
        {
            var user = userService.GetUser(action.UserId);

            if (user == null) {
                throw new Exception("user not found");
            }

            userService.ResetPassword(user);
        }
    }
}
```

### CommandRouter

The `CommandRouter` is meant to help execute Commands without knowing at execution time exactly how to create the Handler and all its dependencies.

It abstracts CommandHandler location/creation/execution so that callers don't need to know about the Handlers. Since `IFunction<TResult>` is generic, the `CommandRouter` can infer Function result types from the Command's type.

```csharp
var command =
    new AuthenticateUserCommand {
        UserId = UserId,
        Password = Password
    };

var result = commandRouter.ExecuteFunction(command);

Assert.True(result.IsAuthenticated);
```

#### Managing your Commands and Handlers using Ninject

:point_right: Requires [ImsHealth.Patterns.Ninject](https://imshealth.myget.org/feed/internal/package/nuget/ImsHealth.Patterns.Ninject) nuget package

The `CommandRouter` can be configured use **Ninject** to register Handlers for your Commands and inject the Handlers' dependencies. The `ICommandRouter` you get from the **Ninject** Kernel will resolve CommandHandlers and their dependencies using your **Ninject** bindings.
See [ImsHealth.Patterns.Ninject's README](https://gitlab.imshealth.com/tools/ImsHealth.Patterns/blob/master/src/ImsHealth.Patterns.Ninject/README.md#ninject-providers) for more examples.

```csharp
// Bind all dependencies for all registered Handlers
Bind<IUserService>().ToConstant(userService);

// Register your CommandHandlers
Bind<IActionHandler<ResetPasswordAction>>()
    .To<ResetPasswordAction.Handler>();

// Register NinjectCommandHandlerFactory as the ICommandHandlerFactory used by your CommandRouter
Bind<ICommandHandlerFactory>()
    .To<NinjectCommandHandlerFactory>()
    .InSingletonScope();

// Register your CommandRouter
Bind<ICommandRouter>()
    .To<CommandRouter>()
    .InSingletonScope();
```

## Domain Events

Based on the classic [PubSub](https://en.wikipedia.org/wiki/Publish%E2%80%93subscribe_pattern) pattern with some C# help.

### Basic Example

:point_right: Requires [ImsHealth.Patterns](https://imshealth.myget.org/feed/internal/package/nuget/ImsHealth.Patterns) nuget package

#### Step 1: Define your event

```csharp
public class UserAuthenticatedEvent : IEvent
{
    public object EventSource { get; private set; }

    public DateTime EventTime { get; private set; } = DateTime.Now;

    public User User { get; private set; }

    public UserAuthenticatedEvent(
        object eventSource,
        User user
    )
    {
        EventSource = eventSource;
        User = user;
    }
}
```

#### Step 2: Register a handler

```csharp
// Define the handler
public class TrackAuthenticatedUsersEventHandler : IEventHandler<UserAuthenticatedEvent>
{
    public void HandleEvent(UserAuthenticatedEvent e)
    {
        // Track the User
        log.Info("User logged in: " + e.User.Name);
    }
}

// Register the Handler type for a specific Event type
EventBus.Default.Register<UserAuthenticatedEvent, TrackAuthenticatedUsersEventHandler>();
```

#### Step 3: Trigger your event

```csharp
EventBus.Default.Trigger(new UserAuthenticatedEvent(this, user));
```

### Event Handlers

You can register various types of Event Handlers for a given event.

#### `Action<TEvent>`

The specified `Action<TEvent>` is executed each time the event is triggered.

```csharp
EventBus.Default.Register<UserAuthenticatedEvent>(e => log.Info("User authenticated: " + e.User));
```

#### Singleton `IEventHandler<TEvent>`

The same `handlerInstance` is used each time the event is triggered.

```csharp
var handlerInstance = new TrackAuthenticatedUsersEventHandler();
EventBus.Default.Register<UserAuthenticatedEvent>(handlerInstance);
```

#### New `IEventHandler<TEvent>` for each trigger

A new instance of `TEventHandler` is created each time the event is triggered.
`TEventHandler` must be an `IEventHandler<TEvent>` and have a default constructor.

```csharp
EventBus.Default.Register<UserAuthenticatedEvent, TrackAuthenticatedUsersEventHandler>();
```

#### Use a custom `IEventHandlerFactory` to create handler instances

The specified instance of `IEventHandlerFactory` is used to create instances of `IEventHandler<TEvent>`.
This is useful if your handlers need injected dependencies.

```csharp
// Define your factory
public class TrackingEventHandlerFactory : IEventHandlerFactory
{
    public IEnumerable<IEventHandler> GetHandlers<TEvent>() where TEvent : IEvent
    {
        if (typeof(TEvent) == typeof(UserAuthenticatedEvent))
        {
            return new [] { new TrackAuthenticatedUsersEventHandler(someDependency) };
        }
        else
        {
            throw new NotSupportedException();
        }
    }
}

// Register your factory
var factory = new TrackingEventHandlerFactory();
EventBus.Default.Register<UserAuthenticatedEvent>(trackingEventHandlerFactory);
```

#### Use `NinjectEventHandlerFactory` to provide handlers from your Ninject container

If your application uses Ninject to manage Dependency Injection, you can leverage your Ninject bindings to provide your Handler instances and manage their lifetimes.

:point_right: Requires [ImsHealth.Patterns.Ninject](https://imshealth.myget.org/feed/internal/package/nuget/ImsHealth.Patterns.Ninject) nuget package

```csharp
// Bind your handlers in your Kernel
Kernel.Bind<IEventHandler<UserAuthenticatedEvent>>()
    .To<TrackAuthenticatedUsersEventHandler>()
    .InSingletonScope();

// Register the NinjectEventHandlerFactory
EventBus.Default.Register<ActivityTaskScheduledEvent>(new NinjectEventHandlerFactory(Kernel));
```