using System;
using Dalamud.Plugin;
using ImGuiNET;
using Dalamud.Configuration;
using Num = System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Game.Command;
using System.Threading.Tasks;


namespace SinglePlayerExperience
{
    public class SinglePlayerExperience : IDalamudPlugin
    {
        public string Name => "Single Player Experience";
        private DalamudPluginInterface pluginInterface;
        public Config Configuration;

        public bool enabled = true;
        public bool config = false;
        public bool switched = false;
        public int countdown = 50;


        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            Configuration = pluginInterface.GetPluginConfig() as Config ?? new Config();
            enabled = Configuration.Enabled;


            this.pluginInterface.CommandManager.AddHandler("/spec", new CommandInfo(Command2)
            {
                HelpMessage = "Shows config for Single Player Experience."
            });

            this.pluginInterface.CommandManager.AddHandler("/spe", new CommandInfo(Command)
            {
                HelpMessage = "Toggles Single Player Experience."
            });

            this.pluginInterface.UiBuilder.OnBuildUi += DrawWindow;
            this.pluginInterface.UiBuilder.OnOpenConfigUi += ConfigWindow;
        }

        public void Dispose()
        {
            this.pluginInterface.UiBuilder.OnBuildUi -= DrawWindow;
            this.pluginInterface.UiBuilder.OnOpenConfigUi -= ConfigWindow;
            this.pluginInterface.CommandManager.RemoveHandler("/spec");
            this.pluginInterface.CommandManager.RemoveHandler("/spe");
        }

        public void Command(string command, string arguments)
        {
            if (enabled)
            {
                switched = true;
                countdown = 50;
            }
            enabled = !enabled;
        }

        public void Command2(string command, string arguments)
        {
            config = !config;
        }

        private void ConfigWindow(object Sender, EventArgs args)
        {
            config = true;
        }

        private void DrawWindow()
        {
            if (!enabled && switched)
            {
                if (countdown == 0)
                {
                    for (var k = 0; k < this.pluginInterface.ClientState.Actors.Length; k++)
                    {
                        var actor = this.pluginInterface.ClientState.Actors[k];

                        if (actor == null)
                            continue;

                        if (actor is Dalamud.Game.ClientState.Actors.Types.PlayerCharacter pc && pluginInterface.ClientState.LocalPlayer != null)
                        {
                            if (actor.ActorId != pluginInterface.ClientState.LocalPlayer.ActorId)
                            {
                                RerenderActor(actor);
                            }
                        }
                    }
                    switched = false;
                }
                else { countdown--; }
            }

            if (enabled && !pluginInterface.ClientState.Condition[Dalamud.Game.ClientState.ConditionFlag.BoundByDuty] && pluginInterface.ClientState.LocalPlayer != null)
            {
                for (var k = 0; k < this.pluginInterface.ClientState.Actors.Length; k++)
                {
                    var actor = this.pluginInterface.ClientState.Actors[k];

                    if (actor == null)
                        continue;

                    if (actor is Dalamud.Game.ClientState.Actors.Types.PlayerCharacter pc)
                    {
                        if (actor.ActorId != pluginInterface.ClientState.LocalPlayer.ActorId)
                        {
                            HideActor(actor);
                        }
                    }
                }
            }



            if (config)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Single Player Experience Config");
                
                if(ImGui.Button("ON"))
                {
                    enabled = true;
                }

                if (ImGui.Button("OFF"))
                {
                    switched = true;
                    enabled = false;
                    countdown = 50;

                }

                if (ImGui.Button("Save and Close Config"))
                {
                    SaveConfig();

                    config = false;
                }
                ImGui.End();

            }


        }

        public void SaveConfig()
        {
            Configuration.Enabled = enabled;
            this.pluginInterface.SavePluginConfig(Configuration);
        }

        private async void RerenderActor(Dalamud.Game.ClientState.Actors.Types.Actor a)
        {
            await Task.Run(async () => {
                try
                {
                    var addrEntityType = a.Address + 0x8C;
                    var addrRenderToggle = a.Address + 0x104;
                    if (a is Dalamud.Game.ClientState.Actors.Types.PlayerCharacter)
                    {
                        Marshal.WriteByte(addrEntityType, 2);
                        Marshal.WriteInt32(addrRenderToggle, 2050);
                        await Task.Delay(100);
                        Marshal.WriteInt32(addrRenderToggle, 0);
                        await Task.Delay(100);
                        Marshal.WriteByte(addrEntityType, 1);
                    }
                    else
                    {
                        Marshal.WriteInt32(addrRenderToggle, 2050);
                        await Task.Delay(10);
                        Marshal.WriteInt32(addrRenderToggle, 0);
                    }
                }
                catch (Exception ex)
                {
                    PluginLog.LogError(ex.ToString());
                }
            });
        }

        private async void HideActor(Dalamud.Game.ClientState.Actors.Types.Actor a)
        {
            await Task.Run(async () => {
                try
                {
                    var addrEntityType = a.Address + 0x8C;
                    var addrRenderToggle = a.Address + 0x104;



                    if (a is Dalamud.Game.ClientState.Actors.Types.PlayerCharacter)
                    {
                        Marshal.WriteByte(addrEntityType, 2);
                        Marshal.WriteInt32(addrRenderToggle, 2050);
                        //await Task.Delay(100);
                        //Marshal.WriteInt32(addrRenderToggle, 0);
                        await Task.Delay(100);
                        Marshal.WriteByte(addrEntityType, 1);
                    }

                    else
                    {
                        Marshal.WriteInt32(addrRenderToggle, 2050);
                        //await Task.Delay(10);
                        //Marshal.WriteInt32(addrRenderToggle, 0);
                    }
                }
                catch (Exception ex)
                {
                    PluginLog.LogError(ex.ToString());
                }
            });
        }
    }

    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;

    }

}
