﻿using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace H.Formatters
{
    /// <summary>
    /// A formatter that uses <see cref="JsonConvert"/> inside for serialization/deserialization
    /// </summary>
    public class JsonFormatter : IFormatter
    {
        /// <summary>
        /// Serializes using <see cref="JsonConvert"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<byte[]> SerializeAsync(object? obj, CancellationToken cancellationToken = default)
        {
            if (obj == null)
            {
                return Task.FromResult(Array.Empty<byte>());
            }

            var json = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(json);

            return Task.FromResult(bytes);
        }

        /// <summary>
        /// Deserializes using <see cref="JsonConvert"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<T> DeserializeAsync<T>(byte[]? bytes, CancellationToken cancellationToken = default)
        {
            if (bytes == null || !bytes.Any())
            {
#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
                return Task.FromResult<T>(default);
#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
            }

            var json = Encoding.UTF8.GetString(bytes);
            var obj = JsonConvert.DeserializeObject<T>(json);

            return Task.FromResult(obj);
        }
    }
}
