using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SQLite;

namespace BlueScan.Services
{
    public class Persist : IPersist
    {
        private static readonly string DatabaseLocation =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Capture.db3");

        private readonly SemaphoreSlim _sync = new SemaphoreSlim(1, 1);

        public async Task SaveAsync(IEnumerable<ScanResultDataItem> data)
        {
            await _sync.WaitAsync();
            try
            {
                var database = new SQLiteAsyncConnection(DatabaseLocation);
                await database.CreateTableAsync<ScanResultDataItem>();

                // if connected to internet try and send to service
                // at any point there's a failure redirect to SQLite

                // for now we'll just store to SQL lite
                foreach (var scanResultDataItem in data)
                {
                    // we will insert each item instead of insert all so we can catch
                    // if/when anything fails mid insert
                    await database.InsertAsync(scanResultDataItem);
                }

                await database.CloseAsync();
            }
            catch (Exception e)
            {
               // Crashes.TrackError(e);
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task SynchronizeAsync()
        {
            await _sync.WaitAsync();
            
                // check can we connect to the web service

            try
            {
                // check if anything's in the database and if so, 
                // try and sync with online
                var database = new SQLiteAsyncConnection(DatabaseLocation);
                var items = await database.Table<ScanResultDataItem>().ToListAsync();
                foreach (var scanResultDataItem in items)
                {
                    // try to persist each item unless service can let us know 
                    // at which point failures might occur

                    // if saved successfully then delete from db
                    await database.DeleteAsync(scanResultDataItem);
                }
            }
            catch (Exception e)
            {
                //Crashes.TrackError(e);
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task ClearLocalAsync()
        {
            await _sync.WaitAsync();
            try
            {
                if (File.Exists(DatabaseLocation))
                {
                    File.Delete(DatabaseLocation);
                }
            }
            finally
            {
                _sync.Release();
            }                
        }
    }
}