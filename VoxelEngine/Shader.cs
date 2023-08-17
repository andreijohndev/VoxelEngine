using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace VoxelEngine
{
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        private Shader(int handle, Dictionary<string, int> uniformLocations)
        {
            this.Handle = handle;
            this._uniformLocations = uniformLocations;
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        /// <summary>
        /// Set a uniform int on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <param name="data">The data to set.</param>
        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Set a uniform float on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <param name="data">The data to set.</param>
        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        /// <summary>
        /// Sets a uniform Matrix4 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <param name="data">The data to set.</param>
        /// <remarks>
        /// <para>
        /// The matrix is transposed before being sent to the shader.
        /// </para>
        /// </remarks>
        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        /// <summary>
        /// Set a uniform Vector3 on this shader.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <param name="data">The data to set.</param>
        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }

        public static Shader LoadShader(string vertPath, string fragPath)
        {
            int handle;
            Dictionary<string, int> uniformLocations;
            var shaderSource = LoadSource(vertPath);

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, shaderSource);
            CompileShader(vertexShader);

            shaderSource = LoadSource(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            handle = GL.CreateProgram();

            GL.AttachShader(handle, vertexShader);
            GL.AttachShader(handle, fragmentShader);

            LinkProgram(handle);

            GL.DetachShader(handle, vertexShader);
            GL.DetachShader(handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            uniformLocations = new Dictionary<string, int>();

            for (int i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(handle, i, out _, out _);
                var location = GL.GetUniformLocation(handle, key);

                uniformLocations.Add(key, location);
            }

            return new Shader(handle, uniformLocations);
        }

        public static Shader LoadShaderFromResource(string vertName, string fragName)
        {
            int handle;
            Dictionary<string, int> uniformLocations;
            var shaderSource = LoadSourceFromResource(vertName);

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, shaderSource);
            CompileShader(vertexShader);

            shaderSource = LoadSourceFromResource(fragName);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            handle = GL.CreateProgram();

            GL.AttachShader(handle, vertexShader);
            GL.AttachShader(handle, fragmentShader);

            LinkProgram(handle);

            GL.DetachShader(handle, vertexShader);
            GL.DetachShader(handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            GL.GetProgram(handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            uniformLocations = new Dictionary<string, int>();

            for (int i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(handle, i, out _, out _);
                var location = GL.GetUniformLocation(handle, key);

                uniformLocations.Add(key, location);
            }

            return new Shader(handle, uniformLocations);
        }

        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);

            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occured whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
                throw new Exception($"Error occured whilst linking Program({program})");
        }

        private static string LoadSource(string path)
        {
            using (var stream = new StreamReader(path, Encoding.UTF8))
            {
                return stream.ReadToEnd();
            }
        }

        private static string LoadSourceFromResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
