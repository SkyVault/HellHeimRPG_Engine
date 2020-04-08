using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace HellHeimRPG
{
    class Shader {
        int _programId = 0;
        int _vsId = 0;
        int _fsId = 0; 

        public int Id { get => _programId; }

        void Compile(int id, ShaderType type) {
            GL.CompileShader(id); 
            string log = GL.GetShaderInfoLog(id);

            if (log != System.String.Empty) {
                Console.WriteLine($"{type}::ERROR:: {log}");
            }
        }

        public Shader(string vsCode, string fsCode) {
            _vsId = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(_vsId, vsCode);
            Compile(_vsId, ShaderType.VertexShader);

            _fsId = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(_fsId, fsCode); 
            Compile(_fsId, ShaderType.FragmentShader);

            _programId = GL.CreateProgram();
            GL.AttachShader(_programId, _vsId);
            GL.AttachShader(_programId, _fsId);
            GL.LinkProgram(_programId);

            //TODO(Dustin): Handle program linking errors

        }

        ~Shader() {
            if (_programId == 0) return;

            GL.DetachShader(_programId, _fsId);
            GL.DetachShader(_programId, _vsId);
            GL.DeleteShader(_vsId);
            GL.DeleteShader(_fsId);
        }

        public int GetLoc(string name) {
            int result = GL.GetUniformLocation(_programId, name);

            if (result < 0) {
                Console.WriteLine($"Failed to find uniform: {name}");
            }

            return result;
        }
        
        public void SetUniform(int uid, float x, float y, float z) {
            GL.Uniform3(uid, x, y, z);
        }

        public void SetUniform(int uid, float f) {
            GL.Uniform1(uid, f);
        }

        public void SetUniform(int uid, float[] f) {
            switch(f.Length) {
                case 0: { break; }// TODO(Dustin): ERROR
                case 1: { GL.Uniform1(uid, f[0]); break; }
                case 2: { GL.Uniform2(uid, f[0], f[1]); break; }
                case 3: { GL.Uniform3(uid, f[0], f[1], f[2]); break; }
                default: { GL.Uniform4(uid, f[0], f[1], f[2], f[3]); break; }
            }
        }
 
        public void SetUniform(int uid, Matrix4 matrix) {
            GL.UniformMatrix4(uid, false, ref matrix);
        }

        public void Use() => GL.UseProgram(_programId);
        public void Use(int id) => GL.UseProgram(id);
        public void UnUse() => GL.UseProgram(0);

        public void Bind(Action func) {
            GL.UseProgram(_programId); 
            func(); 
            GL.UseProgram(0);
        }
    }
}