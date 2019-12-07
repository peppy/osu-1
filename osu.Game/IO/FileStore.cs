﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using osu.Framework.Extensions;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.IO
{
    /// <summary>
    /// Handles the Store and retrieval of Files/FileSets to the database backing
    /// </summary>
    public class FileStore : DatabaseBackedStore
    {
        public readonly IResourceStore<byte[]> Store;

        public new Storage Storage => base.Storage;

        public FileStore(IDatabaseContextFactory contextFactory, Storage storage)
            : base(contextFactory, storage.GetStorageForDirectory(@"files"))
        {
            Store = new StorageBackedResourceStore(Storage);
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="FileInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Results from the provided query.</returns>
        public IEnumerable<FileInfo> QueryFiles(Expression<Func<FileInfo, bool>> query) => ContextFactory.Get().All<FileInfo>().Where(f => f.ReferenceCount > 0).Where(query);

        public FileInfo Add(Stream data, bool reference = true)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                string hash = data.ComputeSHA2Hash();

                var info = usage.Context.All<FileInfo>().FirstOrDefault(f => f.Hash == hash);

                if (info == null)
                {
                    usage.Context.Add(info = new FileInfo { Hash = hash });
                }

                string path = info.StoragePath;

                // we may be re-adding a file to fix missing store entries.
                bool requiresCopy = !Storage.Exists(path);

                if (!requiresCopy)
                {
                    // even if the file already exists, check the existing checksum for safety.
                    using (var stream = Storage.GetStream(path))
                        requiresCopy |= stream.ComputeSHA2Hash() != hash;
                }

                if (requiresCopy)
                {
                    data.Seek(0, SeekOrigin.Begin);

                    using (var output = Storage.GetStream(path, FileAccess.Write))
                        data.CopyTo(output);

                    data.Seek(0, SeekOrigin.Begin);
                }

                if (reference)
                    Reference(info);

                return info;
            }
        }

        public void Reference(params FileInfo[] files)
        {
            if (files.Length == 0) return;

            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                foreach (var f in files.GroupBy(f => f.ID))
                    f.First().ReferenceCount += f.Count();
            }
        }

        public void Dereference(params FileInfo[] files)
        {
            if (files.Length == 0) return;

            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                foreach (var f in files.GroupBy(f => f.ID))
                    f.First().ReferenceCount -= f.Count();
            }
        }

        public override void Cleanup()
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                var context = usage.Context;

                foreach (var f in context.All<FileInfo>().Where(f => f.ReferenceCount < 1))
                {
                    try
                    {
                        Storage.Delete(f.StoragePath);
                        context.Remove(f);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, $@"Could not delete beatmap {f}");
                    }
                }
            }
        }
    }
}
