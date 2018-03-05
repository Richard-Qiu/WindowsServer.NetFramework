﻿namespace Microsoft.AspNet.WebApi.MessageHandlers.Compression.Models
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Interfaces;

    /// <summary>
    /// Represents compressed HTTP content.
    /// </summary>
    public class CompressedContent : HttpContent
    {
        /// <summary>
        /// The original content
        /// </summary>
        private readonly HttpContent originalContent;

        /// <summary>
        /// The compressor
        /// </summary>
        private readonly ICompressor compressor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressedContent"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="compressor">The compressor.</param>
        public CompressedContent(HttpContent content, ICompressor compressor)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            if (compressor == null)
            {
                throw new ArgumentNullException("compressor");
            }

            this.originalContent = content;
            this.compressor = compressor;

            this.CopyHeaders();
        }

        /// <summary>
        /// Determines whether the HTTP content has a valid length in bytes.
        /// </summary>
        /// <param name="length">The length in bytes of the HTTP content.</param>
        /// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="length" /> is a valid length; otherwise, false.</returns>
        protected override bool TryComputeLength(out long length)
        {
            length = -1;

            return false;
        }

        /// <summary>
        /// serialize to stream as an asynchronous operation.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <param name="context">Information about the transport (channel binding token, for example). This parameter may be null.</param>
        /// <returns>Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.</returns>
        protected async override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            // Read and compress stream
            using (var ms = new MemoryStream(await this.originalContent.ReadAsByteArrayAsync()))
            {
                var compressedLength = await this.compressor.Compress(ms, stream).ConfigureAwait(false);

                // Content-Length: {size}
                this.Headers.ContentLength = compressedLength;   
            }
        }

        /// <summary>
        /// Adds the headers.
        /// </summary>
        private void CopyHeaders()
        {
            // Allow: {methods}
            foreach (var allow in this.originalContent.Headers.Allow)
            {
                this.Headers.Allow.Add(allow);
            }

            // Content-Disposition: {disposition-type}; {disposition-param}
            this.Headers.ContentDisposition = this.originalContent.Headers.ContentDisposition;

            // Content-Encoding: {content-encodings}
            this.Headers.ContentEncoding.Add(compressor.EncodingType);

            // Content-Language: {languages}
            foreach (var language in this.originalContent.Headers.ContentLanguage)
            {
                this.Headers.ContentLanguage.Add(language);
            }

            // Content-Type: {media-types}
            this.Headers.ContentType = this.originalContent.Headers.ContentType;

            // Content-Location: {uri}
            this.Headers.ContentLocation = this.originalContent.Headers.ContentLocation;

            // Content-MD5: {md5-digest}
            this.Headers.ContentMD5 = this.originalContent.Headers.ContentMD5;

            // Content-Range: {range}
            this.Headers.ContentRange = this.originalContent.Headers.ContentRange;

            // Content-Type: {media-types}
            this.Headers.ContentType = this.originalContent.Headers.ContentType;

            // Expires: {http-date}
            this.Headers.Expires = this.originalContent.Headers.Expires;

            // LastModified: {http-date}
            this.Headers.LastModified = this.originalContent.Headers.LastModified;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Net.Http.HttpContent" /> and optionally disposes of the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            // Dispose original stream
            this.originalContent.Dispose();

            base.Dispose(disposing);
        }
    }
}