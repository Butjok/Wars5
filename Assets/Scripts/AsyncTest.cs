using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Butjok.CommandLine;
using UnityEngine;

public class AsyncTest : MonoBehaviour {

    [Command]
    public void RunTest(int n) {
        var tasks = new List<Task<int>>();
        tasks.Add(Task.Run(() => Fibonacci(n)));
        StartCoroutine(WaitForTasks(tasks));
    }

    public IEnumerator WaitForTasks(List<Task<int>> tasks) {
        while (tasks.Any(t => !t.IsCompleted))
            yield return null;
        Debug.Log("Tasks completed!");
    }

    public int Fibonacci(int n) {
        int a = 0, b = 1;
        for (int i = 0; i < n; i++) {
            int temp = a;
            a = b;
            b = temp + b;
        }
        Thread.Sleep(1000);
        return a;
    }
}