using ImGuiOpenTK;
using ImGuiNETWidget;
using ImGuiNET;
using Harp;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using Dear_ImGui_Sample;
using HellHeimRPG.Editor;

namespace HellHeimRPG {
    class Program { 
        static void Main(string[] args) { 
            var game = new Game();
            var editor = new Editor.Editor();

            ImGuiController controller = null;

            using (var win = new GameWindow(1920 - 200, 1080 - 200)) {
                win.Load += (sender, e) => { 
                    win.VSync = VSyncMode.On; 

                    game.Load(); 

                    controller = new ImGuiController(1920 - 200, 1080 - 200);
                };

                win.Resize += (sender, e) => {
                    game.Resize((win.Width, win.Height));
                    GL.Viewport(0, 0, win.Width, win.Height);

                    controller.WindowResized(win.Width, win.Height);
                };

                win.KeyPress += (sender, e) => {
                    controller.PressChar(e.KeyChar);
                };

                win.UpdateFrame += (sender, e) => {
                    game.Tick(e.Time);
                };

                win.RenderFrame += (sender, e) => { 
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                    GL.Enable(EnableCap.DepthTest); 

                    game.Render();

                    controller.Update(win, (float)e.Time);

                    editor.Render();

                    controller.Render();

                    win.SwapBuffers(); 
                };

                win.Run(85.0f);
            }
        }
    }
}
