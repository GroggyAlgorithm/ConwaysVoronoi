using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
        new int[2] 
        {
            1,0 //right
        },
        new int[2] 
        {
            0,-1 //left
        },
        new int[2] 
        {
            -1,0//down
        },
        new int[2] 
        {
            1,1 //up right
        },
        new int[2] 
        {
            -1,-1//down,left
        },
        new int[2] 
        {
            1,-1//up left
        },
        
        new int[2] 
        {
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
    static int startingAliveCount = 156;
    static int width = 110;
    static int height = 30;
    static int octaveCount = 2;

    private const int MF_BYCOMMAND = 0x00000000;
    public const int SC_CLOSE = 0xF060;
    public const int SC_MINIMIZE = 0xF020;
    public const int SC_MAXIMIZE = 0xF030;
    public const int SC_SIZE = 0xF000;

    [DllImport("user32.dll")]
    public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();

    


    static readonly Dictionary<string, string> possibleArgs = new Dictionary<string, string>()
    {
        {
            "--he","\t\t: Displays this menu"
        },
        {
            "--max","\t\t: The next integer passed will be maximum cells allowed to be alive"
        },
        {
            "--seed","\t\t: The next integer passed will be the rng seed"
        },
        {
            "--ne","\t\t: The next integer passed will be the amount of neighors for a cell"
        },
        {
            "--val","\t\t: The next float passed will be maximum cells allowed to be alive"
        },
        {
            "--starting","\t: The next integer passed will be the amount of cells to begin alive"
        },
        {
            "--octave","\t: The next integer passed will be the amount of noise octaves to use"
        },
        {
            "--gain","\t\t: The gain for the noise levels"
        },
        {
            "--amp", "\t\t: The amplitude for the noise levels"
        },
        {
            "--freq", "\t\t: The frequency for the noise levels"
        },
        {
            "--scale", "\t: The scale for the noise levels"
        },
        {
            "--dis", "\t\t: The displasement values for the Voronoi noise"
        },
        {
            "--test", "\t\t: Prints a set of random values"
        },
        {
            "--game", "\t\t: Runs Conways Game of Life"
        }
        ,
        {
            "--print", "\t: Prints a map set from noise values"
        }
        ,
        {
            "--algo", "\t\t: Sets the algorithm to use"
        }
        
    };



    


    /// <summary>
    /// Main function
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        activeVN = new VoronoiNoise();
        Console.CursorVisible = false;
        var inactiveColor = Console.ForegroundColor;
        Console.Title = "Conways Voronoi";

        StopResizing();

        Console.SetWindowSize(width, height);


        if(args.Length > 0)
        {
            Console.Clear();
            bool isExitable = false;

            for (var i = 0; i < args.Length && i < possibleArgs.Count; i+=1)
            {
                if(args[i] == "--he" || args[i] == "--game" || args[i] == "--print" || args[i] == "--test" || i >= args.Length - 1)
                {
                    SetSettingFromArg(args[i], "");

                    if(args[i] == "--game" || args[i] == "--print" || args[i] == "--test")
                    {
                        isExitable = true;
                    }
                }
                else
                {
                    SetSettingFromArg(args[i], args[i + 1]);
                    i += 1;
                }
                
                
            }
                
            Console.WriteLine("Press enter to continue");
            Console.ReadLine();

            if(isExitable)
            {
                return;
            }
            
        }
        Console.Clear();
        
        

        for (; ; )
        {
            currentInput = "";

            
            PrintSettings();

            Console.WriteLine("Enter input(exit or x for exit, s for settings, p for map print, t for numerical test, r for run): ");
            currentInput = Console.ReadLine().ToLower();

            if (currentInput != null)
            {
                if (currentInput == "exit" || currentInput == "x")
                {
                    Console.WriteLine("Goodbye!");
                    Console.Clear();
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
                        BlankMapInit(ref activeMap, ref activeColors, height, width, inactiveColor);
                        InitializeConwaysGameMaps(ref activeMap, ref activeColors, startingAliveCount, inactiveColor, ConsoleColor.Green);
                        RunConwaysIteration(ref activeMap, ref activeColors, inactiveColor, ConsoleColor.Green);
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



    /// <summary>
    /// Handles setting the arguments passed <inheritdoc this/>
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="argValue"></param>
    static void SetSettingFromArg(string arg, string argValue)
    {
        bool showHelp = false;
        
        switch(arg)
        {
            case "--seed":
                if (!int.TryParse(argValue, out var newSeed))
                {
                    showHelp = true;
                }
                else
                {
                    activeVN.SetSeed(newSeed);
                }
            break;

            case "--max":
                if (!int.TryParse(argValue, out var newMax))
                {
                    showHelp = true;
                }
                else
                {
                    maxActiveCount = newMax;
                }
            break;

            case "--ne":
                if (!int.TryParse(argValue, out var newNe))
                {
                    showHelp = true;
                }
                else
                {
                    neighborsToBeActive = newNe;
                }
            break;

            case "--val":
                if (!float.TryParse(argValue, out var newVal))
                {
                    showHelp = true;
                }
                else
                {
                    valueForActive = newVal;
                }
            break;

            case "--starting":
                if (!int.TryParse(argValue, out var newStart))
                {
                    showHelp = true;
                }
                else
                {
                    startingAliveCount = newStart;
                }
            break;

            case "--octave":
                if (!int.TryParse(argValue, out var newOcts))
                {
                    showHelp = true;
                }
                else
                {
                    octaveCount = newOcts;
                }
            break;

            case "--algo":
                if (!int.TryParse(argValue, out var algo))
                {
                    showHelp = true;
                }
                else if(algo < GRandomAlgorithms.GetAlgorithmCount())
                {
                    activeVN.SetAlgorithm((GRandomAlgorithms.AlgorithmChoices)algo);
                }
                else
                {
                    showHelp = true;
                }
            break;

            case "--dis":
                if (!float.TryParse(argValue, out var newDispl))
                {
                    showHelp = true;
                }
                else
                {
                    activeVN.displacement = newDispl;
                }
            break;

            case "--scale":
                if (!float.TryParse(argValue, out var sc))
                {
                    showHelp = true;
                }
                else
                {
                    activeVN.scale = sc;
                }
            break;

            case "--freq":
                if (!float.TryParse(argValue, out var fr))
                {
                    showHelp = true;
                }
                else
                {
                    activeVN.frequency = fr;
                }
            break;

            case "--amp":
                if (!float.TryParse(argValue, out var am))
                {
                    showHelp = true;
                }
                else
                {
                    activeVN.amplitude = am;
                }
            break;

            case "--gain":
                if (!float.TryParse(argValue, out var gn))
                {
                    showHelp = true;
                }
                else
                {
                    activeVN.gain = gn;
                }
            break;
            
            case "--print":
                BlankMapInit(ref activeMap, ref activeColors, height, width, Console.ForegroundColor);
                InitializeConwaysGameMaps(ref activeMap, ref activeColors, startingAliveCount, Console.ForegroundColor, ConsoleColor.Green);
                RunConwaysIteration(ref activeMap, ref activeColors, Console.ForegroundColor, ConsoleColor.Green);
            break;

            case "--game":
                RunConwaysThreads();
            break;

            case "--test":
                Console.WriteLine("\nVoronoi Float Values\n");
                for (var i = 0; i < 10; i++)
                {
                    Console.WriteLine("Float value " + i + ":" +activeVN.NextFloat());
                }
            break;



            default:
                showHelp = true;
                break;

        }

        if(showHelp)
        {
            Console.WriteLine("\n\nBad Argument " + arg + " " + argValue );
            Console.WriteLine("\n\t------Conways Voronoi------");
            foreach(var kvp in possibleArgs)
            {
                Console.WriteLine(kvp.Key + " " + kvp.Value);
            }

            for (var i = 0; i < GRandomAlgorithms.GetAlgorithmCount(); i++)
            {
                Console.WriteLine("\t\t: " + i + " = " + (GRandomAlgorithms.AlgorithmChoices)i);
            }

            Console.WriteLine("\n\t------Conways Voronoi------\n");
        }
    }



    /// <summary>
    /// Disables all ability to resize screen manually
    /// </summary>
    static void StopResizing()
    {
        IntPtr handle = GetConsoleWindow();
        IntPtr sysMenu = GetSystemMenu(handle, false);

        if (handle != IntPtr.Zero)
        {
            // DeleteMenu(sysMenu, SC_CLOSE, MF_BYCOMMAND);
            DeleteMenu(sysMenu, SC_MINIMIZE, MF_BYCOMMAND);
            DeleteMenu(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND);
            DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
        }
    }



    /// <summary>
    /// Prints the settings view
    /// </summary>
    static void PrintSettings()
    {
        Console.WriteLine("\n\n\t\t------Conways Voronoi------");
        Console.WriteLine("\n\tSelected Algorithm: " + activeVN._selected_algo);
        Console.WriteLine("\tDiplacement of: " + activeVN.displacement);
        Console.WriteLine("\tScale of: " + activeVN.scale);
        Console.WriteLine("\tFrequency of: " + activeVN.frequency);
        Console.WriteLine("\tAmplitude of: " + activeVN.amplitude);
        Console.WriteLine("\tGain of: " + activeVN.gain);
        Console.WriteLine("\tSeed of: " + activeVN.seed);
        Console.WriteLine("\tNeighbors to be active: " + neighborsToBeActive);
        Console.WriteLine("\tMinimum value to be active: " + valueForActive);
        Console.WriteLine("\tMinimum game cells to start alive: " + startingAliveCount);
        Console.WriteLine("\tMaximum game cells allowed: " + maxActiveCount);
        Console.WriteLine("\tOctave count of: " + octaveCount);
        Console.WriteLine("\n");
    }


    
    /// <summary>
    /// Handles the settings menus
    /// </summary>
    static void EnterSettings()
    {
        while (true)
        {
            Console.Clear();
            PrintSettings();
            Console.WriteLine("Enter input(exit or x for exit, 0 for octaves, 1 for displacement, 2 for algorithm, 3 for seed,");
            Console.WriteLine("4 for The amount of neighbors to be active, 5 for The minimum value need to start as active,");
            Console.WriteLine("6 for starting alive count, 7 for Noise Scale, 8 for Noise Frequency, 9 for Noise Amplitude,\n10 for Noise Gain,\n11 for Maximum Alive Count)\n\n:");



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
                            
                            for (var i = 0; i < GRandomAlgorithms.GetAlgorithmCount(); i++)
                            {
                                options += "\nOption " + i + ": ";
                                options += (GRandomAlgorithms.AlgorithmChoices)i;
                            }
                            Console.WriteLine(options);
                            newInput = Console.ReadLine();

                            if (int.TryParse(newInput, out var algoOut))
                            {
                                if (algoOut < GRandomAlgorithms.GetAlgorithmCount())
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
                            if (!int.TryParse(newInput, out startingAliveCount))
                            {
                                Console.WriteLine("Bad Input");
                                startingAliveCount = 100;
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



    /// <summary>
    /// Initializes maps to blank
    /// </summary>
    /// <param name="activeMap">The map of active values</param>
    /// <param name="activeColors">The map of active colors</param>
    /// <param name="height">The height of maps</param>
    /// <param name="width">The width of the maps</param>
    /// <param name="defaultColor">The color to default to</param>
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



    /// <summary>
    /// Initializes the map for conways game
    /// </summary>
    /// <param name="activeMap">Reference to the active map for active valus</param>
    /// <param name="activeColors">Reference to the active colors</param>
    /// <param name="startingAliveCount">The alive count when starting</param>
    /// <param name="defaultColor">The default and the inactive color</param>
    /// <param name="activeColor">The console color when active</param>
    static void InitializeConwaysGameMaps(ref string[,] activeMap, ref ConsoleColor[,] activeColors, int startingAliveCount, ConsoleColor defaultColor, ConsoleColor activeColor)
    {
        BlankMapInit(ref activeMap, ref activeColors, height-2, width-1, defaultColor);

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

        while (startingAliveCount >= 0)
        {
            var currentX = activeVN.Range(4, activeMap.GetLength(1) - 3);
            var currentY = activeVN.Range(4, activeMap.GetLength(0) - 3);
            activeMap[currentY, currentX] = activeString;
            activeColors[currentY, currentX] = activeColor;
            startingAliveCount--;

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
                            startingAliveCount--;
                        }
                    }
                }

                
            }

            
        }


        
    }



    /// <summary>
    /// Runs an iteration of conways game
    /// </summary>
    /// <param name="activeMap">Reference to the active map for active valus</param>
    /// <param name="activeColors">Reference to the active colors</param>
    /// <param name="inactiveColor">The console color when inactive</param>
    /// <param name="activeColor">The console color when active</param>
    static void RunConwaysIteration(ref string[,] activeMap, ref ConsoleColor[,] activeColors, ConsoleColor inactiveColor, ConsoleColor activeColor)
    {
        int totalActiveCount = 0;
        Console.ForegroundColor = inactiveColor;
        Console.SetCursorPosition(0,0);
        Console.Write("\tCulling Events happen after " + maxActiveCount + " cells are alive.\n\tPress s To Start/Stop, Enter r to exit, or p to restart");

        for(var y = 4; y < activeMap.GetLength(0) - 3 && y < Console.BufferHeight; y++)
        {
            for(var x = 4; x < activeMap.GetLength(1) - 3 && x < Console.BufferWidth; x++)
            {
                var activeNeighborsCount = 0;

                if(totalActiveCount > maxActiveCount)
                {
                    KillRandomUntilCountMet(ref totalActiveCount, inactiveColor);
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
                        activeColors[y, x] = inactiveColor;


                    }
                }
                else
                {
                    activeMap[y, x] = inactiveString;
                    activeColors[y, x] = inactiveColor;
                }

                Console.SetCursorPosition(x,y);
                Console.ForegroundColor = activeColors[y,x];
                Console.Write(activeMap[y,x]);
            }
        }



        // Console.SetCursorPosition(1,Console.BufferHeight - 20);
        Console.Write("\r");
        Thread.Sleep(10);
    }



    /// <summary>
    /// Randomly kills active positions until the total active count is less than the max active count
    /// </summary>
    /// <param name="totalActiveCount"></param>
    /// <param name="inactiveColor"></param>
    static void KillRandomUntilCountMet(ref int totalActiveCount, ConsoleColor inactiveColor)
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
                    activeColors[ny, nx] = inactiveColor;
                }
            }
        }
    }



    /// <summary>
    /// Runs conways game
    /// </summary>
    static void RunConwaysGame()
    {
        
        var inactiveColor = Console.ForegroundColor;
        bool isRunning = false;
        activeMap = new string[height,width];
        activeColors =  new ConsoleColor[height,width];
        currentInput = "";
        Console.SetWindowSize(width,height+10);
        Console.Clear();
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.CursorVisible = false;

        InitializeConwaysGameMaps(ref activeMap, ref activeColors, startingAliveCount, inactiveColor, aliveColor);
        Thread.Sleep(5);
        Console.Clear();
        RunConwaysIteration(ref activeMap, ref activeColors, inactiveColor, aliveColor);
        // Console.SetCursorPosition(0, Console.BufferHeight - 5);
        // Console.Write("Culling Events happen after " + maxActiveCount + " cells are alive. Press s To Start/Stop, Enter r to stop or p to restart");

        while(currentInput != "r")
        {

            if(Console.BufferHeight < activeMap.GetLength(0) || Console.BufferWidth < activeMap.GetLength(1))
            {
                Console.SetWindowSize(width,height+10);
            }

            if(currentInput == "r" || currentInput == "x" || currentInput == "exit")
            {
                break;
            }
            else if(currentInput == "s")
            {
                isRunning = !isRunning;
                RunConwaysIteration(ref activeMap, ref activeColors, inactiveColor, aliveColor);
                currentInput = "";
                // Console.SetCursorPosition(0, Console.BufferHeight - 5);
                // Console.Write("Culling Events happen after " + maxActiveCount + " cells are alive. Press s To Start/Stop, Enter r to stop or p to restart");
            }
            else if (currentInput == "p")
            {
                InitializeConwaysGameMaps(ref activeMap, ref activeColors, startingAliveCount, inactiveColor, aliveColor);
                Thread.Sleep(5);
                Console.Clear();
                RunConwaysIteration(ref activeMap, ref activeColors, inactiveColor, aliveColor);
                Thread.Sleep(5);
                // Console.SetCursorPosition(0, Console.BufferHeight - 5);
                // Console.Write("Culling Events happen after " + maxActiveCount + " cells are alive. Press s To Start/Stop, Enter r to stop or p to restart");
                currentInput = "";
                isRunning = false;
            }
            else if(isRunning == true)
            {
                RunConwaysIteration(ref activeMap, ref activeColors, inactiveColor, aliveColor);
            }




        }
    
    
        Console.ForegroundColor = inactiveColor;
    }



    /// <summary>
    /// Launches the threads for conways game
    /// </summary>
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




    /// <summary>
    /// Input listener to run while conways game is running
    /// </summary>
    static void RunningInputListener()
    {
        

        while(currentInput != "r")
        {
            

            //If key is available...
            if(Console.KeyAvailable)
            {
                currentInput = Console.ReadLine().ToLower();

                if(currentInput == "r" || currentInput == "x" || currentInput == "exit")
                {
                    break;
                }
            }

            Thread.Sleep(10);
        }
    }

    


}