﻿using ARK_Server_Manager.Lib;
using ARK_Server_Manager.Lib.ViewModel;
using ARK_Server_Manager.Lib.ViewModel.RCON;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ARK_Server_Manager
{
    public enum PlayerSortType
    {
        Online,
        Name,
        Tribe
    }

    [Flags]
    public enum PlayerFilterType
    {
        None            = 0,
        Offline         = 0x1,
        Online          = 0x2,
        Banned          = 0x4,
        Whitelisted     = 0x8
    }
    
    public enum InputMode
    {
        Command,
        Global,
        Broadcast,
    }

    public class ScrollToBottomAction : TriggerAction<RichTextBox>
    {
        protected override void Invoke(object parameter)
        {
            AssociatedObject.ScrollToEnd();
        }
    }

    public class RCONOutput_CommandTime : Run
    {
        public RCONOutput_CommandTime() : this(DateTime.Now) { }
        public RCONOutput_CommandTime(DateTime time) : base($"[{time.ToString("g")}] ") { }
    }

    public class RCONOutput_TimedCommand : Span
    {
        protected RCONOutput_TimedCommand() : base()
        {
            base.Inlines.Add(new RCONOutput_CommandTime());
        }

        public RCONOutput_TimedCommand(Inline output) : this()
        {            
            base.Inlines.Add(output);
        }

        public RCONOutput_TimedCommand(string output) : this(new Run(output)) { }

    }

    public class RCONOutput_Comment : Run
    {
        public RCONOutput_Comment(string value) : base(value) { }
    }

    public class RCONOutput_ChatSend : RCONOutput_TimedCommand
    {
        public RCONOutput_ChatSend(string target, string output) : base($"[{target}] {output}") { }
    }
    public class RCONOutput_Broadcast : RCONOutput_ChatSend
    {
        public RCONOutput_Broadcast(string output) : base("ALL", output) { }
    }

    public class RCONOutput_ConnectionChanged : RCONOutput_TimedCommand
    {
        public RCONOutput_ConnectionChanged(bool isConnected) : base(isConnected ? "Connection established." : "Connection lost.") { }
    }

    public class RCONOutput_Command : RCONOutput_TimedCommand
    {
        public RCONOutput_Command(string text) : base(text) { }
    };

    public class RCONOutput_NoResponse : RCONOutput_TimedCommand
    {
        public RCONOutput_NoResponse() : base("Command returned no data") { }
    };

    public class RCONOutput_CommandOutput : RCONOutput_TimedCommand
    {
        public RCONOutput_CommandOutput(string text) : base(text) { }
    };

    /// <summary>
    /// Interaction logic for RCON.xaml
    /// </summary>
    public partial class RCONWindow : Window
    {
        public bool ScrollOnNewInput
        {
            get { return (bool)GetValue(ScrollOnNewInputProperty); }
            set { SetValue(ScrollOnNewInputProperty, value); }
        }

        public static readonly DependencyProperty ScrollOnNewInputProperty = DependencyProperty.Register(nameof(ScrollOnNewInput), typeof(bool), typeof(RCONWindow), new PropertyMetadata(true));

        public ICollectionView  PlayersView
        {
            get { return (ICollectionView)GetValue(PlayersViewProperty); }
            set { SetValue(PlayersViewProperty, value); }
        }

        public static readonly DependencyProperty PlayersViewProperty = DependencyProperty.Register(nameof(PlayersView), typeof(ICollectionView), typeof(RCONWindow), new PropertyMetadata(null));


        public PlayerSortType PlayerSorting
        {
            get { return (PlayerSortType)GetValue(PlayerSortingProperty); }
            set { SetValue(PlayerSortingProperty, value); }
        }

        public static readonly DependencyProperty PlayerSortingProperty = DependencyProperty.Register(nameof(PlayerSorting), typeof(PlayerSortType), typeof(RCONWindow), new PropertyMetadata(PlayerSortType.Online));

        public PlayerFilterType PlayerFiltering
        {
            get { return (PlayerFilterType)GetValue(PlayerFilteringProperty); }
            set { SetValue(PlayerFilteringProperty, value); }
        }

        public static readonly DependencyProperty PlayerFilteringProperty = DependencyProperty.Register(nameof(PlayerFiltering), typeof(PlayerFilterType), typeof(RCONWindow), new PropertyMetadata(PlayerFilterType.Online | PlayerFilterType.Offline | PlayerFilterType.Banned | PlayerFilterType.Whitelisted));


        public Server Server
        {
            get { return (Server)GetValue(ServerProperty); }
            set { SetValue(ServerProperty, value); }
        }

        public static readonly DependencyProperty ServerProperty = DependencyProperty.Register(nameof(Server), typeof(Server), typeof(RCONWindow), new PropertyMetadata(null));


        public ServerRCON ServerRCON
        {
            get { return (ServerRCON)GetValue(ServerRCONProperty); }
            set { SetValue(ServerRCONProperty, value); }
        }

        public static readonly DependencyProperty ServerRCONProperty = DependencyProperty.Register(nameof(ServerRCON), typeof(ServerRCON), typeof(RCONWindow), new PropertyMetadata(null));



        public InputMode CurrentInputMode
        {
            get { return (InputMode)GetValue(CurrentInputModeProperty); }
            set { SetValue(CurrentInputModeProperty, value); }
        }

        public static readonly DependencyProperty CurrentInputModeProperty = DependencyProperty.Register(nameof(CurrentInputMode), typeof(InputMode), typeof(RCONWindow), new PropertyMetadata(InputMode.Command));

        public RCONWindow(Server server)
        {
            InitializeComponent();
            this.Server = server;
            this.ServerRCON = new ServerRCON(server);
            this.ServerRCON.RegisterCommandListener(RenderRCONCommandOutput);
            this.PlayersView = CollectionViewSource.GetDefaultView(this.ServerRCON.Players);
            this.PlayersView.Filter = p =>
            {
                var player = p as PlayerInfo;

                return (this.PlayerFiltering.HasFlag(PlayerFilterType.Online) && player.IsOnline) ||
                       (this.PlayerFiltering.HasFlag(PlayerFilterType.Offline) && !player.IsOnline) ||
                       (this.PlayerFiltering.HasFlag(PlayerFilterType.Banned) && player.IsBanned) ||
                       (this.PlayerFiltering.HasFlag(PlayerFilterType.Whitelisted) && player.IsWhitelisted);
            };

            var notifier = new PropertyChangeNotifier(this.ServerRCON, ServerRCON.StatusProperty, (s, a) =>
            {
                this.RenderConnectionStateChange(a);
            });
            this.DataContext = this;

            AddCommentsBlock(
                "Enter commands or chat into the box at the bottom.",
                "In Command mode, everything you enter will be a normal admin command",
                "In Broadcast mode, everytihng you enter will be a global broadcast",
                "You may always prefix a command with / to be treated as a command and not chat.",
                "Right click on players in the list to access player commands",
                "Type /help to get help");

            this.ConsoleInput.Focus();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            this.ServerRCON.DisposeAsync().DoNotWait();
            base.OnClosing(e);
        }

        public ICommand ClearLogsCommand
        {
            get
            {
                return new RelayCommand<object>(
                    execute: (_) =>
                    {
                        string logsDir = String.Empty;
                        try
                        {
                            logsDir = App.GetProfileLogDir(this.Server.Runtime.ProfileSnapshot.ProfileName);
                            Directory.Delete(logsDir, true);
                        }
                        catch (Exception)
                        {
                            // Ignore any failures here, best effort only.
                        }

                        MessageBox.Show($"Logs in {logsDir} deleted.", "Logs deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                    },
                    canExecute: (sort) => true
                );
            }
        }

        public ICommand ViewLogsCommand
        {
            get
            {
                return new RelayCommand<object>(
                    execute: (_) =>
                    {
                        string logsDir = String.Empty;
                        try
                        {
                            logsDir = App.GetProfileLogDir(this.Server.Runtime.ProfileSnapshot.ProfileName);
                            Process.Start(logsDir);
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show($"Unable to open the logs directory at {logsDir}.  Please make sure this directory exists and that you have permission to access it.\nException: {ex.Message}", "Can't open logs", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    },
                    canExecute: (sort) => true
                );
            }
        }

        public ICommand SortPlayersCommand
        {
            get
            {
                return new RelayCommand<PlayerSortType>(
                    execute: (sort) => 
                    {
                        this.PlayersView.SortDescriptions.Clear();
                        switch(sort)
                        {
                            case PlayerSortType.Name:
                                this.PlayersView.ToggleSorting(nameof(PlayerInfo.SteamName));
                                break;

                            case PlayerSortType.Online:
                                this.PlayersView.ToggleSorting(nameof(PlayerInfo.IsOnline), ListSortDirection.Descending);
                                break;

                            case PlayerSortType.Tribe:
                                this.PlayersView.ToggleSorting(nameof(PlayerInfo.TribeName));
                                break;
                        }                        
                    },
                    canExecute: (sort) => true
                );
            }
        }       

        public ICommand FilterPlayersCommand
        {
            get
            {
                return new RelayCommand<PlayerFilterType>(
                    execute: (filter) => 
                    {
                        this.PlayerFiltering ^= filter;
                        this.PlayersView.Refresh();
                    },
                    canExecute: (filter) => true
                );
            }
        }

        public ICommand KillPlayerCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => { this.ServerRCON.IssueCommand($"KillPlayer {player.SteamId}"); },
                    canExecute: (player) => false // player != null && player.IsOnline
                );
            }
        }

        public ICommand KickPlayerCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => { this.ServerRCON.IssueCommand($"KickPlayer {player.SteamId}"); },
                    canExecute: (player) => player != null && player.IsOnline
                    );
            }
        }

        public ICommand BanPlayerCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => { var command = player.IsBanned ? "Unban" : "Ban" ;  this.ServerRCON.IssueCommand($"{command} {player.ArkData.CharacterName}"); },
                    canExecute: (player) => true
                    );
            }
        }

        public ICommand WhitelistPlayerCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => { var command = player.IsWhitelisted ? "DisallowPlayerToJoinNoCheck" : "AllowPlayerToJoinNoCheck"; this.ServerRCON.IssueCommand($"{command} {player.ArkData.CharacterName}"); },
                    canExecute: (player) => true
                );
            }
        }

        public ICommand ViewPlayerProfileCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => { Process.Start($"http://steamcommunity.com/profiles/{player.SteamId}"); },
                    canExecute: (player) => true 
                );
            }
        }

        public ICommand ViewPlayerTribeCommand
        {
            get
            {
                return new RelayCommand<PlayerInfo>(
                    execute: (player) => { },
                    canExecute: (player) => false //player != null && !String.IsNullOrWhiteSpace(player.TribeName
                    );

            }
        }

        private void RenderConnectionStateChange(DependencyPropertyChangedEventArgs a)
        {
            var oldStatus = (ServerRCON.ConsoleStatus)a.OldValue;
            var newStatus = (ServerRCON.ConsoleStatus)a.NewValue;
            if(oldStatus != newStatus)
            {
                Paragraph p = new Paragraph();
                if (newStatus == ServerRCON.ConsoleStatus.Connected)
                {
                    p.Inlines.Add(new RCONOutput_ConnectionChanged(true));
                }
                else
                {
                    p.Inlines.Add(new RCONOutput_ConnectionChanged(false));
                }

                AddBlockContent(p);
            }
        }

        private void RenderRCONCommandOutput(ServerRCON.ConsoleCommand command)
        {
            //
            // Format output
            //
            Paragraph p = new Paragraph();

            if (!command.suppressCommand)
            {
                foreach (var element in FormatCommandInput(command))
                {
                    p.Inlines.Add(element);
                }
            }

            if (!command.suppressOutput)
            {
                foreach (var element in FormatCommandOutput(command))
                {
                    p.Inlines.Add(element);
                }
            }

            if (!(command.suppressCommand && command.suppressOutput))
            {
                if (p.Inlines.Count > 0)
                {
                    AddBlockContent(p);
                }
            }
        }

        private void AddBlockContent(Block b)
        {
            ConsoleContent.Blocks.Add(b);            
        }

        private IEnumerable<Inline> FormatCommandInput(ServerRCON.ConsoleCommand command)
        {
            if (command.command.Equals("broadcast", StringComparison.OrdinalIgnoreCase))
            {
                yield return new RCONOutput_Broadcast(command.args);
            }
            else
            {
                yield return new RCONOutput_Command($"> {command.rawCommand}");
            }

            if(!command.suppressOutput && command.lines.Count() > 0)
            {
                yield return new LineBreak();
            }
        }

        private void AddCommentsBlock(params string[] lines)
        {
            var p = new Paragraph();
            bool firstLine = true;

            foreach (var output in lines)
            {
                var trimmed = output.TrimEnd();
                if (!firstLine)
                {
                    p.Inlines.Add(new LineBreak());                    
                }

                firstLine = false;

                p.Inlines.Add(new RCONOutput_Comment(output));
            }

            AddBlockContent(p);
        }

        private IEnumerable<Inline> FormatCommandOutput(ServerRCON.ConsoleCommand command)
        {
            bool firstLine = true;
            
            foreach (var output in command.lines)
            {
                var trimmed = output.TrimEnd();
                if(!firstLine)
                {
                    yield return new LineBreak();
                }
                firstLine = false;

                if (output == ServerRCON.NoResponseOutput)
                {
                    yield return new RCONOutput_NoResponse();
                }
                else
                {
                    yield return new RCONOutput_CommandOutput(trimmed);
                }
            }
        }

        private void ConsoleInput_KeyUp(object sender, KeyEventArgs e)
        {            
            if(e.Key == Key.Enter)
            {
                var textBox = (TextBox)sender;
                var effectiveMode = this.CurrentInputMode;
                var commandText = textBox.Text.Trim();
                if (commandText.StartsWith("/help"))
                {
                    AddCommentsBlock(
                        "Known commands:",
                        "   AllowPlayerToJoinNoCheck <player> - Adds the specified player to the whitelist",
                        "   Ban <player> - Adds the specified player to the banned list",
                        "   Broadcast <message> - Sends a message to everyone",
                        "   DestroyAll <class name> - Destroys ALL creatures of the specified class",
                        "   destroyallenemies - Destroys ALL dinosaurs on the map",
                        "   DisallowPlayerToJoinNoCheck <player> - Removes the specified player from the whitelist",
                        "   giveitemnumtoplayer <player> <itemnum> <quantity> <quality> <recipe> - Gives items to a player",
                        "   KickPlayer <steam id> - Kicks the specified user from the server",
                        "   KillPlayer <steam id> - Kills the player (and mount), without leaving a body",
                        "   ListPlayers - Lists the current players",
                        "   PlayersOnly - Toggles all creature movement and crafting",
                        "   saveworld - Saves the world to disk",
                        "   serverchat <message> - Sends a message to global chat",
                        "   SetMessageOfTheDay <message> - Sets the message of the day",
                        "   settimeofday <hour>:<minute>[:<second>] - Sets the time using 24-hour format",
                        "   slomo <factor> - Sets the passage of time.  Lower values slow time",
                        "   Unban <player> - Remove the specified player from the banned list",
                        "where:",
                        "   <player> specifies the character name of the player",
                        "   <steam id> is the long numerical id of the player"
                        );
                }
                else
                {
                    if (commandText.StartsWith("/"))
                    {
                        effectiveMode = InputMode.Command;
                        commandText = commandText.Substring(1);
                    }

                    switch (effectiveMode)
                    {
                        case InputMode.Broadcast:
                            this.ServerRCON.IssueCommand($"broadcast {commandText}");
                            break;

                        case InputMode.Global:
                            this.ServerRCON.IssueCommand($"serverchat {commandText}");
                            break;

                        case InputMode.Command:
                            this.ServerRCON.IssueCommand(commandText);
                            break;

#if false
                    case InputMode.Chat:
                        this.ServerRCON.IssueCommand(textBox.Text);
                        break;
#endif
                    }
                }

                textBox.Text = String.Empty;
            }
        }
    }
}
