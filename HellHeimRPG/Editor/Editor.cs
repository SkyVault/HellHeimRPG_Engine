using ImGuiNET;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Text;
using Quaternion = OpenTK.Quaternion;
using Vector3 = OpenTK.Vector3;
using HellHeimRPG.Filters;

namespace HellHeimRPG.Editor
{
    class Editor
    {
        CommandWindow commandWindow = new CommandWindow();

        Vector3 selColor = new Vector3();

        public void DoEditor()
        {

        }

        public void DoEntityCreateModel()
        {
            if (ImGui.BeginPopupModal("Spawn?"))
            {
                if (ImGui.Button("Spawn"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }

        public void DoVector3Editor(ref Vector3 v, int id)
        {
            System.Numerics.Vector3 vector = new System.Numerics.Vector3(v.X, v.Y, v.Z);
            ImGui.DragFloat3($"##{id}", ref vector, 0.01f);
            v.X = vector.X;
            v.Y = vector.Y;
            v.Z = vector.Z;
        }

        public void DoInspector()
        {
            if (ImGui.Button("Create"))
            {
                // Launch the create object dialog
                if (!ImGui.IsPopupOpen("Spawn?"))
                {
                    ImGui.OpenPopup("Spawn?");
                }
            }

            foreach (var entity in Ecs.It.Each())
            {
                var components = entity.Components;

                ImGui.PushID(entity.Id.Index);
                ImGui.AlignTextToFramePadding();

                bool nodeOpen = ImGui.TreeNode(
                    $"<entity:{entity.Tag}>"
                );

                if (nodeOpen)
                {
                    int i = 0;
                    foreach (var key in components.Keys)
                    {
                        ImGui.PushID(i);
                        if (ImGui.TreeNode(key.ToString()))
                        {
                            object comp = components[key];

                            var fields = comp.GetType().GetFields();
                            var properties = comp.GetType().GetProperties();

                            int id = 0;

                            foreach (var member in fields)
                            {
                                ImGui.Text(member.Name);
                            }

                            foreach (var member in properties)
                            {

                                if (member.CanRead && !member.CanWrite)
                                {
                                    ImGui.Text(member.Name);
                                    if (member.PropertyType == typeof(float))
                                    {
                                        ImGui.SameLine();
                                        ImGui.Text(((float)member.GetValue(comp, null)).ToString(CultureInfo.InvariantCulture));
                                    }
                                    if (member.PropertyType == typeof(int))
                                    {
                                        ImGui.SameLine();
                                        ImGui.Text(((int)member.GetValue(comp, null)).ToString(CultureInfo.InvariantCulture));
                                    }
                                }

                                if (!member.CanRead || !member.CanWrite)
                                    continue;

                                ImGui.Text(member.Name);

                                if (member.PropertyType == typeof(Vector3))
                                {
                                    Vector3 v = (Vector3)member.GetValue(comp, null);
                                    ImGui.SameLine();
                                    DoVector3Editor(ref v, id++);
                                    member.SetValue(comp, v);
                                }
                                else if (member.PropertyType == typeof(bool))
                                {
                                    var b = (bool)member.GetValue(comp, null);
                                    ImGui.SameLine();
                                    ImGui.Checkbox($"{member.Name}", ref b);
                                    member.SetValue(comp, b);
                                }
                                else if (member.PropertyType == typeof(Quaternion))
                                {
                                    Quaternion q = (Quaternion)member.GetValue(comp, null);

                                    ImGui.SameLine();

                                    var x = q.X; var y = q.Y; var z = q.Z; var w = q.W;

                                    var ax = (float)Math.Atan2(2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z);
                                    var ay = (float)Math.Atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z);
                                    var az = (float)Math.Asin(2 * x * y + 2 * z * w);

                                    Vector3 rotv = new Vector3(
                                        (float)MathHelper.RadiansToDegrees(ax + Math.PI),
                                        (float)MathHelper.RadiansToDegrees(ay + Math.PI),
                                        (float)MathHelper.RadiansToDegrees(az + Math.PI));

                                    DoVector3Editor(ref rotv, id++);

                                    var q2 = Quaternion.FromEulerAngles(new Vector3(
                                        MathHelper.DegreesToRadians(rotv.X % 360),
                                        MathHelper.DegreesToRadians(rotv.Y % 360),
                                        MathHelper.DegreesToRadians(rotv.Z % 360)
                                    ));

                                    member.SetValue(comp, q2);
                                }
                                else if (member.PropertyType == typeof(Material))
                                {
                                    ImGui.SameLine();

                                    Material material = (Material)(member.GetValue(comp, null));

                                    System.Numerics.Vector4 color =
                                        new System.Numerics.Vector4(material.Diffuse.R, material.Diffuse.G, material.Diffuse.B, material.Diffuse.A);

                                    if (ImGui.ColorPicker4("Diffuse", ref color))
                                    {
                                        material.Diffuse = new Color4(color.X, color.Y, color.Z, color.W);
                                    }

                                }
                            }

                            ImGui.TreePop();
                        }
                        ImGui.PopID();

                        i++;
                    }

                    ImGui.TreePop();
                }

                ImGui.PopID();
            }
        }

        private bool open = false;

        public void DoSelection()
        {
            var sfbo = Art.It.GetFbo("selection");

            if (Input.It.LeftClicked)
            {
                sfbo.Bind(() =>
                {
                    GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

                    var (mx, my) = Input.It.MousePosition;

                    var scale_w = (float)Game.Resolution.W / (float)Game.WindowSize.Item1;
                    var scale_h = (float)Game.Resolution.H / (float)Game.WindowSize.Item2;

                    var smx = (int)Math.Floor(mx * scale_w);
                    var smy = (int)Math.Floor(my * scale_h);

                    var color = new byte[4];
                    GL.ReadPixels(smx, Game.Resolution.H - smy, 1, 1, PixelFormat.Rgb, PixelType.UnsignedByte, color);

                    selColor = new Vector3(color[0] / 255f, color[1] / 255f, color[2] / 255f);
                    Console.WriteLine($"{mx} {my} {smx} {smy} {scale_w} {scale_h}");
                });
            };
        }

        public void Render()
        {
            if (Input.It.ToggleEditor)
                open = !open;

            if (!open) return;

            DoSelection();

            ImGui.ShowDemoWindow();

            commandWindow.Do();

            if (ImGui.Begin("Editor"))
            {
                ImGui.ColorButton("SELECTION COLOR", new System.Numerics.Vector4(selColor.X, selColor.Y, selColor.Z, 1.0f));
                if (ImGui.TreeNode("Frame buffers"))
                {
                    foreach (var (name, fbo) in Art.It.Fbos())
                    {
                        if (ImGui.TreeNode(name))
                        {
                            ImGui.Image((IntPtr)fbo.ColorBuffer,
                                new System.Numerics.Vector2(200 * (16f / 9f), 200),
                                new System.Numerics.Vector2(0, 0),
                                new System.Numerics.Vector2(1, -1),
                                new System.Numerics.Vector4(1, 1, 1, 1),
                                new System.Numerics.Vector4(1, 1, 1, 1));
                        }

                        ImGui.SameLine();

                        if (ImGui.Button($"{name}##Show")) { Renderer.ScreenFBO = name; }
                    }
                }

                ImGui.End();
            }

            if (ImGui.Begin("Inspector"))
            {
                DoEntityCreateModel();
                DoInspector();
                ImGui.End();
            }
        }
    }
}
