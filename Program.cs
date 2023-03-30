using System;
using System.Collections;
using System.Collections.Generic;

public static class ConwaysVoronoi
{
    
    

    const string activeString = "\u2588";
    const string mountainString = "\u2589";
    const string inactiveString = "\u2591";

    const string mapSideLeftString = "\u2551 ";
    const string mapSideRightString = " \u2551\n";

    const string mapSideTopLeft = "\u2554";
    const string mapSideTopRight = "\u2557\n";
    const string mapSideBottomLeft = "\u255A";
    const string mapSideBottomRight = "\u255D\n";
    const string mapSideTopMiddle = "\u2550";
    const string mapSideBottomMiddle = "\u2550";

    static readonly List<int[]> Directions2D = new List<int[]>()
    {
        new int[2] 
        {
            0,1 //up
        },
        new int[2] {
            1,0 //right
        },
        new int[2] {
            0,-1 //left
        },
        new int[2] {
            -1,0//down
        },
        new int[2] {
            1,1 //up right
        },
        new int[2] {
            -1,-1//down,left
        },
        new int[2] {
            1,-1//up left
        },
        
        new int[2] {
            -1,1//down right
        },
    };

    static ConsoleColor aliveColor = ConsoleColor.Red;
    static VoronoiNoise activeVN;
    static ConsoleColor[,] activeColors;
    static string[,] activeMap;
    static List<int[]> AllActivePositions;
    static string currentInput = "";
    static int maxActiveCount = 200;
    static int neighborsToBeActive = 3;
    static float valueForActive = 1;
    static int liveCount = 156;
    static int width = 100;
    static int height = 25;
    static int octaveCount = 2;


    

    public static void Main(string[] args)
    {
        activeVN = new VoronoiNoise();
        Console.Clear();

        var savedColor = Console.ForegroundColor;
        Console.Title = "Conways Voronoi";

        for (; ; )
        {
            currentInput = "";

            Console.SetWindowSize(width, height);
            Console.WriteLine("------Conways Voronoi------");
            Console.WriteLine("\nSelected Algorithm: " + activeVN._selected_algo);
            Console.WriteLine("Diplacement of: " + activeVN.displacement);
            Console.WriteLine("Scale of: " + activeVN.scale);
            Console.WriteLine("Frequency of: " + activeVN.frequency);
            Console.WriteLine("Amplitude of: " + activeVN.amplitude);
            Console.WriteLine("Gain of: " + activeVN.gain);
            Console.WriteLine("Seed of: " + activeVN.seed);
            Console.WriteLine("Neighbors to be active: " + neighborsToBeActive);
            Console.WriteLine("Minimum value to be active: " + valueForActive);
            Console.WriteLine("Minimum game cells to start alive: " + liveCount);
            Console.WriteLine("Maximum game cells allowed: " + maxActiveCount);
            Console.WriteLine("Octave count of: " + octaveCount);
            Console.WriteLine("\n");

            Console.WriteLine("Enter input(exit or x for exit, s for settings, p for map print, t for numerical test, r for run):");
            currentInput = Console.ReadLine().ToLower();

            if (currentInput != null)
            {
                if (currentInput == "exit" || currentInput == "x")
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                switch (currentInput)
                {
                    case "r":
                        currentInput = "";
                        RunConwaysThreads();
                        Console.Clear();
                        break;
                    case "p":
                        currentInput = "";
                        Console.Clear();
                        BlankMapInit(ref activeMap, ref activeColors, height, width, savedColor);
                        InitializeConwaysGameMaps(ref activeMap, ref activeColors, liveCount, savedColor, ConsoleColor.Green);
                        RunConwaysIteration(ref activeMap, ref activeColors, savedColor, ConsoleColor.Green);
                        break;

                    case "t":
                        currentInput = "";
                        Console.WriteLine("\nVoronoi Float Values\n");
                        for (var i = 0; i < 10; i++)
                        {
                            Console.WriteLine("Float value " + i + ":" +activeVN.NextFloat());
                        }
                        break;

                    case "s":
                        currentInput = "";
                        EnterSettings();
                        Console.Clear();
                    break;
                };

                currentInput = "";
            }
        }
    }




    static void EnterSettings()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("------Conways Voronoi------");
            Console.WriteLine("\nSelected Algorithm: " + activeVN._selected_algo);
            Console.WriteLine("Diplacement of: " + activeVN.displacement);
            Console.WriteLine("Scale of: " + activeVN.scale);
            Console.WriteLine("Frequency of: " + activeVN.frequency);
            Console.WriteLine("Amplitude of: " + activeVN.amplitude);
            Console.WriteLine("Gain of: " + activeVN.gain);
            Console.WriteLine("Seed of: " + activeVN.seed);
            Console.WriteLine("Neighbors to be active: " + neighborsToBeActive);
            Console.WriteLine("Minimum value to be active: " + valueForActive);
            Console.WriteLine("Minimum game cells to start alive: " + liveCount);
            Console.WriteLine("Maximum game cells allowed: " + maxActiveCount);
            Console.WriteLine("Octave count of: " + octaveCount);
            Console.WriteLine("\n");
            Console.WriteLine("Enter input(exit or x for exit, 0 for octaves, 1 for displacement, 2 for algorithm, 3 for seed,");
            Console.WriteLine("4 for The amount of neighbors to be active, 5 for The minimum value need to start as active,");
            Console.WriteLine("6 for starting alive count, 7 for Noise Scale, 8 for Noise Frequency, 9 for Noise Amplitude, 10 for Noise Gain,\n11 for Maximum Alive Count):");



            currentInput = Console.ReadLine().ToLower();

            if (currentInput != null)
            {
                if (currentInput == "exit" || currentInput == "x")
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                if (int.TryParse(currentInput, out var inputAsInt))
                {
                    string newInput = "";
                    switch (inputAsInt)
                    {
                        case 0:
                            Console.WriteLine("Enter an Octave Count Integer:");
                            newInput = Console.ReadLine();

                            if (!int.TryParse(newInput, out octaveCount))
                            {
                                Console.WriteLine("Bad Input");
                                octaveCount = 2;
                            }
                            break;
                        case 1:
                            Console.WriteLine("Enter a Displacement Float:");
                            newInput = Console.ReadLine();

                            if (!float.TryParse(newInput, out activeVN.displacement))
                            {
                                Console.WriteLine("Bad Input");
                                activeVN.displacement = 1;
                            }
                            break;

                        case 2:
                            Console.WriteLine("Enter an Interger to select algorithm:");
                            string options = "";
                            for (var i = 0; i < 14; i++)
                            {
                                options += "\nOption " + i + ": ";
                                options += (GRandomAlgorithms.AlgorithmChoices)i;
                            }
                            Console.WriteLine(options);
                            newInput = Console.ReadLine();

                            if (int.TryParse(newInput, out var algoOut))
                            {
                                if (algoOut < 14)
                                {
                                    activeVN.SetAlgorithm((GRandomAlgorithms.AlgorithmChoices)algoOut);
                                }
                                else
                                {
                                    Console.WriteLine("Bad Input");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Bad Input");
                            }
                            break;


                        case 3:
                            Console.WriteLine("Enter a Seed Integer:");
                            newInput = Console.ReadLine();
                            if (!int.TryParse(newInput, out var newSeed))
                            {
                                Console.WriteLine("Bad Input");
                            }
                            else
                            {
                                activeVN.SetSeed(newSeed);
                            }
                            break;


                        case 4:
                            Console.WriteLine("Enter an Integer for the amount of neighbors for position to stay active:");
                            newInput = Console.ReadLine();
                            if (!int.TryParse(newInput, out var neighborsToBeActive))
                            {
                                Console.WriteLine("Bad Input");
                                neighborsToBeActive = 3;
                            }

                            break;
                        case 5:
                            Console.WriteLine("Enter a float for the minimum amount for a position to count as active:");
                            newInput = Console.ReadLine();

                            if (!float.TryParse(newInput, out valueForActive))
                            {
                                Console.WriteLine("Bad Input");
                                valueForActive = 1;
                            }
                            break;

                        case 6:
                            Console.WriteLine("Enter an Integer for the amount of cells to start alive in conways game:");
                            newInput = Console.ReadLine();
                            if (!int.TryParse(newInput, out liveCount))
                            {
                                Console.WriteLine("Bad Input");
                                liveCount = 100;
                            }
                            break;

                        case 7:
                            Console.WriteLine("Enter a float for the noise scale:");
                            newInput = Console.ReadLine();
                            if (!float.TryParse(newInput, out var newfloat))
                            {
                                Console.WriteLine("Bad Input");
                            }
                            else
                            {
                                activeVN.scale = newfloat;
                            }
                            break;

                        case 8:
                            Console.WriteLine("Enter a float for the noise frequency:");
                            newInput = Console.ReadLine();
                            if (!float.TryParse(newInput, out var freqFloat))
                            {
                                Console.WriteLine("Bad Input");
                            }
                            else
                            {
                                activeVN.frequency = freqFloat;
                            }
                            break;

                        case 9:
                            Console.WriteLine("Enter a float for the noise amplitude:");
                            newInput = Console.ReadLine();
                            if (!float.TryParse(newInput, out var ampFloat))
                            {
                                Console.WriteLine("Bad Input");
                            }
                            else
                            {
                                activeVN.amplitude = ampFloat;
                            }
                            break;

                        case 10:
                            Console.WriteLine("Enter a float for the noise gain:");
                            newInput = Console.ReadLine();
                            if (!float.TryParse(newInput, out var gainFloat))
                            {
                                Console.WriteLine("Bad Input");
                            }
                            else
                            {
                                activeVN.gain = gainFloat;
                            }
                            break;

                        case 11:
                            Console.WriteLine("Enter an Integer for the maximum game cells allowed to be alive:");
                            newInput = Console.ReadLine();
                            if (!int.TryParse(newInput, out maxActiveCount))
                            {
                                Console.WriteLine("Bad Input");
                                maxActiveCount = 100;
                            }
                            break;

                    };

                }
                else
                {
                    Console.WriteLine("Bad Input");
                }

            }

        }
    }



    static void BlankMapInit(ref string[,] activeMap, ref ConsoleColor[,] activeColors, int height, int width, ConsoleColor defaultColor)
    {
        activeMap = new string[height,width];
        activeColors =  new ConsoleColor[height,width];

        for(var y = 0; y < height; y++)
        {
            for(var x = 0; x < width; x++)
            {
                activeColors[y,x] = defaultColor;
                if(y == 0 || y == height-1)
                {
                    activeMap[y,x] = mapSideBottomMiddle;
                }
                else if(x > 0 && x < width - 1)
                {
                    activeMap[y,x] = inactiveString;
                }
            }
        }

        activeMap[0,0] = mapSideTopLeft;
        activeMap[0,width-1] = mapSideTopRight;
        activeMap[height-1,0] = mapSideBottomLeft;
        activeMap[height-1,width-1] = mapSideBottomRight;
    }



    static void InitializeConwaysGameMaps(ref string[,] activeMap, ref ConsoleColor[,] activeColors, int liveCount, ConsoleColor defaultColor, ConsoleColor activeColor)
    {
        BlankMapInit(ref activeMap, ref activeColors, height, width, defaultColor);

        float[,]? nm = activeVN.SampleNoiseMap(activeMap.GetLength(1) + 1, activeMap.GetLength(0) + 1, octaveCount);
            
        
        
        for (var y = 4; y < activeMap.GetLength(0) - 3; y++)
        {
            for (var x = 4; x < activeMap.GetLength(1) - 3; x++)
            {
                if (nm[x, y] >= valueForActive)
                {
                    activeMap[y, x] = activeString;
                    activeColors[y, x] = activeColor;
                }

            }
        }

        while (liveCount >= 0)
        {
            var currentX = activeVN.Range(4, activeMap.GetLength(1) - 3);
            var currentY = activeVN.Range(4, activeMap.GetLength(0) - 3);
            activeMap[currentY, currentX] = activeString;
            activeColors[currentY, currentX] = activeColor;
            liveCount--;

            foreach(var dir in Directions2D)
            {
                if(dir[0] + currentY >= 0 && dir[0] + currentY < activeMap.GetLength(0))
                {
                    if(dir[1] + currentX >= 0 && dir[1] + currentX < activeMap.GetLength(1))
                    {
                        if(activeVN.BoolValue() == true)
                        {
                            activeMap[dir[0] + currentY, dir[1] + currentX] = activeString;
                            activeColors[dir[0] + currentY, dir[1] + currentX] = activeColor;
                            liveCount--;
                        }
                    }
                }

                
            }

            
        }


        
    }




    static void RunConwaysIteration(ref string[,] activeMap, ref ConsoleColor[,] activeColors, ConsoleColor savedColor, ConsoleColor activeColor)
    {
        int totalActiveCount = 0;
       

        for(var y = 1; y < activeMap.GetLength(0) - 2; y++)
        {
            for(var x = 1; x < activeMap.GetLength(1) - 2; x++)
            {
                var activeNeighborsCount = 0;

                if(totalActiveCount > maxActiveCount)
                {
                    KillRandomUntilCountMet(ref totalActiveCount, savedColor);
                    // x = activeMap.GetLength(1);
                    // y = activeMap.GetLength(0);
                    // break;
                    // activeMap[y, x] = inactiveString;
                    // activeColors[y, x] = savedColor;
                    // continue;
                }

                foreach(var dir in Directions2D)
                {
                    if(activeMap[y+dir[0],x+dir[1]] == activeString)
                    {
                        activeNeighborsCount++;
                    }
                }

                

                if (activeMap[y, x] == activeString || activeMap[y, x] == inactiveString)
                {


                    if (activeNeighborsCount == neighborsToBeActive)
                    {
                        activeMap[y, x] = activeString;
                        activeColors[y, x] = activeColor;
                        totalActiveCount++;

                    }
                    else if (activeNeighborsCount <= 1 || activeNeighborsCount > neighborsToBeActive)
                    {
                        activeMap[y, x] = inactiveString;
                        activeColors[y, x] = savedColor;


                    }
                }
                else
                {
                    activeMap[y, x] = inactiveString;
                    activeColors[y, x] = savedColor;
                }

                Console.SetCursorPosition(x,y);
                Console.ForegroundColor = activeColors[y,x];
                Console.Write(activeMap[y,x]);
            }
        }



        

        Console.Write("\r");
        Thread.Sleep(10);
    }



    static void KillRandomUntilCountMet(ref int totalActiveCount, ConsoleColor savedColor)
    {
        if(totalActiveCount > maxActiveCount)
        {

            int killCount = activeVN.Range((maxActiveCount / 2), maxActiveCount);
            while(totalActiveCount > killCount)
            {
                var nx = activeVN.Range(0, activeMap.GetLength(1) - 2);
                var ny = activeVN.Range(0, activeMap.GetLength(0) - 2);

                if (activeMap[ny, nx] == activeString)
                {
                    totalActiveCount--;
                    activeMap[ny, nx] = inactiveString;
                    activeColors[ny, nx] = savedColor;
                }
            }
        }
    }



    static void RunConwaysGame()
    {
        var savedColor = Console.ForegroundColor;

        activeMap = new string[height,width];
        activeColors =  new ConsoleColor[height,width];
        currentInput = "";

        Console.Clear();
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;
        Console.SetWindowSize(width+10,height+10);

        InitializeConwaysGameMaps(ref activeMap, ref activeColors, liveCount, savedColor, aliveColor);
        Thread.Sleep(5);
        Console.Clear();
        RunConwaysIteration(ref activeMap, ref activeColors, savedColor, aliveColor);
        Console.SetCursorPosition(0, Console.BufferHeight - 1);
        // Console.SetCursorPosition(0,width - 1);
        Console.WriteLine("Press s To Start, Enter r to stop or p to restart:\n\n");

        while(currentInput != "s" && currentInput != "r" && currentInput != "x" && currentInput != "exit" && currentInput != "p");

        while(currentInput != "r")
        {
            
            if(currentInput == "r" || currentInput == "x" || currentInput == "exit")
            {
                break;
            }
            else if (currentInput == "p")
            {
                InitializeConwaysGameMaps(ref activeMap, ref activeColors, liveCount, savedColor, aliveColor);
                Thread.Sleep(5);
                Console.Clear();
                // Console.WriteLine("Enter r to stop or p to restart:\n\n");
                RunConwaysIteration(ref activeMap, ref activeColors, savedColor, aliveColor);
                Thread.Sleep(5);
                Console.SetCursorPosition(0, Console.BufferHeight - 1);
                Console.WriteLine("Press s To Start, Enter r to stop or p to restart:\n\n");
                currentInput = "";
                // while(currentInput != "s" && currentInput != "r" && currentInput != "x" && currentInput != "exit");
            }
            else
            {
                RunConwaysIteration(ref activeMap, ref activeColors, savedColor, aliveColor);
            }

        }
    
    
        Console.ForegroundColor = savedColor;
    }



    static void RunConwaysThreads()
    {
        try
        {

            Thread thread1 = new Thread(new ThreadStart(RunConwaysGame));
            Thread thread2 = new Thread(new ThreadStart(RunningInputListener));
            thread1.Start();
            thread2.Start();
            while(currentInput != "r" && currentInput != "x" && currentInput != "exit")
            {
                Thread.Sleep(0);
            }

            thread2.Join();
            thread1.Join();
            

            currentInput = "";

            Console.Clear();
            Thread.Sleep(50);
            Console.WriteLine("Returning.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Returning.");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("Returning.");
            return;
        }
    }



    static void RunningInputListener()
    {
        while(currentInput != "r")
        {
            currentInput = Console.ReadLine().ToLower();
            if(!String.IsNullOrWhiteSpace(currentInput))
            {
                if(currentInput == "r" || currentInput == "x" || currentInput == "exit")
                {
                    break;
                }
                Console.Clear();
                
            }

            Thread.Sleep(10);
        }
    }

    


}