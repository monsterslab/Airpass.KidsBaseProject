using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.MemoryMappedFiles;
using System.Threading;
using Tools.Singletons;

public class SharedMemoryManager : SingletonUnityEternal<SharedMemoryManager>
{
    private MemoryMappedFile memoryMappedFile;
    private Mutex mutex;

    private void Start()
    {
        memoryMappedFile = MemoryMappedFile.CreateOrOpen("MySharedMemory", 1024);
        mutex = new Mutex(false, "MyMutex");
    }

    public void WriteToSharedMemory(bool data)
    {
        mutex.WaitOne();

        memoryMappedFile.CreateViewAccessor().Write(0, data);

        mutex.ReleaseMutex();
    }
}
