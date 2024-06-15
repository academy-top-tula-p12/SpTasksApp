using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpTasksApp
{
    class Employee
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    public class Example
    {
        static public void WelcomeTask()
        {
            //Task task1 = new(() => Console.WriteLine("Hello world 1"));
            //task1.Start();

            //Task task2 = Task.Factory.StartNew(() => Console.WriteLine("Hello world 2"));

            //Task task3 = Task.Run(() => Console.WriteLine("Hello world 3"));

            //task1.Wait();
            //task2.Wait();
            //task3.Wait();

            var t1 = Task.Run(Loop);

            var t3 = new Task(Loop);
            t3.RunSynchronously();

            var t2 = Task.Run(Loop);

            for (int i = 0; i < 100; i++)
                Console.WriteLine($"Main #{i}");

            t1.Wait();
            t2.Wait();

            void Loop()
            {
                for (int i = 0; i < 100; i++)
                    Console.WriteLine($"Task #{Thread.CurrentThread.ManagedThreadId} - {i}");
            }
        }
        static public void OuterInnerTasks()
        {
            Console.WriteLine("Main start");

            Task outer = Task.Factory.StartNew(() =>
            {
                Console.WriteLine("Outer task start");

                Task inner = Task.Factory.StartNew(() =>
                {
                    Console.WriteLine("Inner task start");
                    Thread.Sleep(2000);
                    Console.WriteLine("Inner task finish");
                }, TaskCreationOptions.AttachedToParent);

                Console.WriteLine("Outer task finish");
            });

            outer.Wait();
            Console.WriteLine("Main finish");
        }
        static public void ArrayTasks()
        {
            Task[] tasks = new Task[]
            {
                new (Loop),
                new (Loop),
                new (Loop),
            };

            foreach (var task in tasks)
                task.Start();

            //foreach (var task in tasks)
            //    task.Wait();

            //Task.WaitAll(tasks);
            Task.WaitAny(tasks);

            void Loop()
            {
                Random random = new Random();
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine($"Task #{Task.CurrentId} - {i}");
                    Thread.Sleep(random.Next(100, 200));
                }
            }
        }
        static public void ResultsTasks()
        {

            Task<int> sum1 = new Task<int>(() => Sum(100));
            sum1.Start();

            int result = sum1.Result;
            Console.WriteLine(result);

            Task<Employee> taskEmpl = new Task<Employee>(() => new Employee() { Name = "Bobby", Age = 27 });
            taskEmpl.Start();

            var e = taskEmpl.Result;


            int Sum(int number)
            {
                int result = 0;
                for (int i = 0; i <= number; i++)
                {
                    Console.WriteLine($"Task #{Task.CurrentId} - {i}");
                    result += i;
                }

                return result;
            }

            
        }
        static public void ContinuationTasks()
        {
            //Task task1 = new(Loop);
            //Task task3 = Task.Run(Loop);


            //Task task2 = task1.ContinueWith((task) => 
            //{
            //    Loop();
            //});

            //task1.Start();

            //task2.Wait();

            Task<int> taskSum = new Task<int>(() => Sum(100));

            Task taskPrint = taskSum.ContinueWith((task) => Print(task.Result));

            taskSum.Start();

            taskPrint.Wait();

            void Loop()
            {
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine($"Task #{Task.CurrentId} - {i}");
                    Thread.Sleep(100);
                }
            }


            void Print(int result)
            {
                Console.WriteLine($"Result = {result}");
            }
            int Sum(int number)
            {
                int result = 0;

                for (int i = 0; i <= number; i++)
                    result += i;

                Thread.Sleep(1000);
                return result;
            }
        }
        static public void ParallelMethods()
        {
            Action[] actions = new[]
            {
                Loop,
                Loop,
                Loop
            };

            List<Employee> employees = new List<Employee>()
            {
                new(){ Name = "Poppy", Age = 33 },
                new(){ Name = "Bobby", Age = 27 },
                new(){ Name = "Sammy", Age = 32 },
                new(){ Name = "Jimmy", Age = 19 },
                new(){ Name = "Benny", Age = 23 },
                new(){ Name = "Tommy", Age = 35 },
            };

            //Parallel.Invoke(actions);

            //Parallel.For(5, 11, LoopParam);

            ParallelLoopResult result = Parallel.ForEach(employees, Print);
            if (result.IsCompleted)
                Console.WriteLine("All employees print");



            void Print(Employee employee, ParallelLoopState state)
            {
                if (employee.Name.Trim().Length == 0)
                    state.Break();
                else
                    Console.WriteLine($"Name: {employee.Name}, Age: {employee.Age}");
            }

            void Loop()
            {
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine($"Task #{Task.CurrentId} - {i}");
                    Thread.Sleep(100);
                }
            }

            void LoopParam(int number)
            {
                for (int i = 0; i < number; i++)
                {
                    Console.WriteLine($"Task #{Task.CurrentId} - {number} {i}");
                    Thread.Sleep(100);
                }
            }
        }
        static public void CancelsTasks()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            Task task = new Task(() => Loop(token), token);

            //try
            //{
            //    task.Start();

            //    Thread.Sleep(2000);
            //    Console.WriteLine("Waiting task...");

            //    cts.Cancel();

            //    task.Wait();
            //}
            //catch(AggregateException ex)
            //{
            //    foreach(Exception e in ex.InnerExceptions)
            //        Console.WriteLine(e.Message);
            //}
            //finally
            //{
            //    cts.Dispose();
            //}


            //Console.WriteLine("Main finish");

            new Task(() =>
            {
                Thread.Sleep(500);

                cts.Cancel();
            }).Start();

            try
            {
                Parallel.For(5,
                        10,
                        new ParallelOptions { CancellationToken = token },
                        LoopParam);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                cts.Dispose();
            }





            void Loop(CancellationToken token)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        Console.WriteLine("Task is break");
                        //return;

                        token.ThrowIfCancellationRequested();
                    }

                    Console.WriteLine($"Task #{Task.CurrentId} - {i}");
                    Thread.Sleep(500);
                }
            }

            void LoopParam(int number)
            {
                for (int i = 0; i <= number; i++)
                {
                    Console.WriteLine($"Task #{Task.CurrentId}: {number} - {i}");
                    Thread.Sleep(500);
                }

            }
        }
    }
}
