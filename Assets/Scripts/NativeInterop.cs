using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Butjok.CommandLine;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class NativeInterop {

    [DllImport("png2.dylib")]
    public static extern IntPtr png_new(int start);

    [DllImport("png2.dylib")]
    public static extern int png_next(IntPtr g);

    [DllImport("png2.dylib")]
    public static extern void png_test(IntPtr g, int count);
    
    [Command]
    public static int count = 100000;

    [Command]
    public static float TestC() {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        
        var g = png_new(2);
        png_test(g, count);
        
        stopWatch.Stop();
        return stopWatch.ElapsedMilliseconds;
    }

    [Command]
    public static float TestCSharp() {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        
        next = 2;
        for (var i = 0; i < count; i++)
            Next();
        
        stopWatch.Stop();
        return stopWatch.ElapsedMilliseconds;
    }

    public static int next = 2;
    public static int Next() {
        for (; ; next++) {
            var isPrime = true;
            for (var j = 2; j < next; j++)
                if (next % j == 0) {
                    isPrime = false;
                    break;
                }
            if (isPrime) 
                return next++;
        }
    }
}