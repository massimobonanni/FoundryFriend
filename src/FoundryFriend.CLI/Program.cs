using FoundryFriend.CLI.Services;
using FoundryFriend.CLI.Utilities;
using FoundryFriend.Core.Configuration;
using FoundryFriend.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.CommandLine;

ConsoleUtility.WriteApplicationBanner();

/// <summary>
/// Configure dependency injection container with required services.
/// Registers session management, GitHub Models service, and client services as singletons.
/// </summary>
var serviceCollection = new ServiceCollection();
serviceCollection.TryAddSingleton<ISessionManager, SessionManager>();
serviceCollection.TryAddTransient<IChatService, ChatService>();

// Build the service provider to resolve dependencies
var serviceProvider = serviceCollection.BuildServiceProvider();

/// <summary>
/// Create the root command with available subcommands for the CLI application.
/// Supports 'set' for configuration, 'translate' for AI text processing, and 'models' for model management.
/// </summary>
var rootCommand = new FoundryFriend.CLI.Commands.RootCommand(serviceProvider);

/// <summary>
/// Parse the command-line arguments and execute the corresponding command handler.
/// </summary>
ParseResult parseResult = rootCommand.Parse(args);
return await parseResult.InvokeAsync();