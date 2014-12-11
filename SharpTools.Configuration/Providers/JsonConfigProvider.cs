using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpTools.Configuration.Attributes;

namespace SharpTools.Configuration.Providers
{
    public class JsonConfigProvider<T> : IProvider<T>
        where T : class, IConfig<T>, new()
    {
        private readonly Stream _output;
        private readonly JsonSerializerSettings _settings;

        public Stream Output { get { return _output; } }

        public JsonConfigProvider() : this(new MemoryStream())
        {
        }

        public JsonConfigProvider(Stream output)
            : this(output, new JsonSerializerSettings())
        {
        }

        public JsonConfigProvider(Stream output, JsonSerializerSettings settings)
        {
            if (output == null)
                throw new ArgumentNullException("output", "Output stream cannot be null!");

            _output   = output;
            _settings = settings;
        }

        /// <summary>
        /// NOTE: This simply returns a default config instance.
        /// </summary>
        public T Read()
        {
            return new T();
        }

        public Task<T> ReadAsync()
        {
            return Task.FromResult(Read());
        }

        public T Read(string config)
        {
            return JsonConvert.DeserializeObject<T>(config, _settings);
        }

        public Task<T> ReadAsync(string config)
        {
            return Task.FromResult(Read(config));
        }

        public void Save(T config)
        {
            // Generate the tokenized json for the config
            var serializer = JsonSerializer.Create(_settings);
            var jobj       = JObject.FromObject(config, serializer);

            // Load the encryption key
            var encryptionKey = config.GetEncryptionKey();
            if (string.IsNullOrWhiteSpace(encryptionKey))
                throw new EncryptConfigException("Invalid encryption key! Must not be null or empty.");

            // Encrypt the values of all EncryptAttribute-decorated properties
            var encryptedProperties = GetPropertiesToEncrypt();
            foreach (var prop in encryptedProperties)
            {
                var jprop   = jobj.Property(prop.Name);
                var val     = jprop.Value<string>();
                jprop.Value = Encrypt(val);
                jobj[prop.Name] = jprop;
            }

            // Write the json to the output stream
            using (var writer     = new StreamWriter(_output, Encoding.UTF8))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jobj.WriteTo(jsonWriter);
            }

            // Seek back to the beginning for reading
            _output.Seek(0, SeekOrigin.Begin);
        }

        public Task SaveAsync(T config)
        {
            return Task.Run(() => Save(config));
        }

        private string Encrypt(string decrypted)
        {
            throw new NotImplementedException();
        }

        private string Decrypt(string encrypted)
        {
            throw new NotImplementedException();
        }

        private PropertyInfo[] GetPropertiesToEncrypt()
        {
            return typeof (T).GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof (EncryptAttribute)))
                .ToArray();
        }
    }
}
