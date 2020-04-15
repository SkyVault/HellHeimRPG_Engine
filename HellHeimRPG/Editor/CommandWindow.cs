using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ImGuiNET;

namespace HellHeimRPG.Editor {
    public class CommandWindow
    {
        private bool open = false;
        private List<string> dsp_history = new List<string>();
        private List<string> cmd_history = new List<string>();

        public void Do() {
            ImGui.SetNextWindowSize(new Vector2(800, 400), ImGuiCond.FirstUseEver);

            ImGui.Begin("Harp Console", ref open);

            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.MenuItem("Close Console")) open = false;
                ImGui.EndPopup();
            }

            // Buttons
            if (ImGui.SmallButton("Enter")) { } ImGui.SameLine();
            if (ImGui.SmallButton("Clear")) { }
            ImGui.Separator();

            float footer_h = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            ImGui.BeginChild("ScrollingRegion", new Vector2(0, -footer_h), false,
                ImGuiWindowFlags.AlwaysHorizontalScrollbar);

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1));

            foreach (var item in dsp_history) {
                ImGui.TextUnformatted(item);
            }

            ImGui.PopStyleVar();
            ImGui.EndChild();
            ImGui.Separator();

            byte[] buff = new byte[800]; 
            if (ImGui.InputText("Input", buff, (uint)buff.Length, ImGuiInputTextFlags.EnterReturnsTrue)) {
                string command = Encoding.ASCII.GetString(buff, 0, buff.Length);
                command = command.Replace(Environment.NewLine, "");

                for (int i = 0; i < command.Length; i++) {
                    if (command[i] == '\0') {
                        command = command.Substring(0, i);
                        break;
                    }
                }

                Console.WriteLine($"'{command}'");

                var result = Game.Harp.Eval(Game.Env, command);
                dsp_history.Add(command);
                dsp_history.Add($"({cmd_history.Count})> {result.ToString()}");
                cmd_history.Add(command);
            }

            ImGui.End();
        }
    }
}
