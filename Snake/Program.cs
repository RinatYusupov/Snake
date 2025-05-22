using System;
using System.Collections.Generic;
using System.IO;

namespace Snake
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ///TODO: записать символы в map и считать снова
            /// ЛИБО!!!!!!!!!!!!!!!! сравнивать хэш директа с хэшами uniqueCoordinates и удалять при совпадении. + и - при съедании исчезнут. Карта будет нужна только для стен

            // map содержит только стены
            // хэш директа сравнивается с хэшсэтом знаков и удаляется при совпадении

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "map.txt");

            char[,] map = ReadMap(filePath);

            Random random = new Random();

            int[,] snakeElementsPositions = GetStartPosition(map, random);
            int[] direction = new int[] { snakeElementsPositions[0, 0], snakeElementsPositions[0, 1] };

            ConsoleKeyInfo pressedKey = new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false);

            int enlargerSignsAmount = 15;
            int reducerSignsAmount = 15;

            bool isWork = true;
            bool canMove = false;
            bool isLastMove = false;

            HashSet<(int, int)> usedCoordinates = GetWallCoordinates(map);
            HashSet<(int, int)> enlagersCoordinates = SetUniqueCoordinates(ref usedCoordinates, direction, enlargerSignsAmount, map, random);
            HashSet<(int, int)> reducersCoordinates = SetUniqueCoordinates(ref usedCoordinates, direction, reducerSignsAmount, map, random);

            Console.WindowWidth = 120;
            Console.WindowHeight = 50;
            Console.CursorVisible = false;

            do
            {
                Renderer(map, snakeElementsPositions, enlagersCoordinates, reducersCoordinates);
                ShowInfo(pressedKey, direction, snakeElementsPositions, enlagersCoordinates, reducersCoordinates);
                pressedKey = Console.ReadKey();

                direction = GetDirect(pressedKey, snakeElementsPositions, map, ref canMove);

                bool isDirectionClear = CompareCoordinates(enlagersCoordinates, reducersCoordinates, direction, ref snakeElementsPositions, ref isLastMove);

                isWork = CanPlay(direction, ref snakeElementsPositions, isWork, ref canMove, enlagersCoordinates, reducersCoordinates, ref isLastMove); //рефакторинг - оставить только проверки. некстПозишн отдельно
                //не могу вытащить isLastMove раньше CompareCoordinates, чтобы проверить CanPlay, потому что количество символов меняется после него
                snakeElementsPositions = GetNextPositions(snakeElementsPositions, direction, isDirectionClear);

                Console.Clear();
            } while (isWork);

            Console.Clear();
            Console.WriteLine("GAME OVER");
            Console.ReadKey();
        }

        static char[,] ReadMap(string filePath)
        {
            string[] file = File.ReadAllLines(filePath);
            char[,] map = new char[GetMaxLengthOfLine(file), file.Length];

            for (int x = 0; x < map.GetLength(0); x++)
                for (int y = 0; y < map.GetLength(1); y++)
                    map[x, y] = file[y][x];

            return map;
        }

        static int GetMaxLengthOfLine(string[] lines)
        {
            int maxLength = lines[0].Length;

            for (int i = 0; i < lines.Length; i++)
                if (lines[i].Length > maxLength)
                    maxLength = lines[i].Length;

            return maxLength;
        }

        static int[,] EnlargeLength(int[,] elementsPositions)
        {
            int[,] enlargedLengthCoordinates = new int[elementsPositions.GetLength(0) + 1, 2];

            for (int i = 0; i < elementsPositions.GetLength(0); i++)
            {
                enlargedLengthCoordinates[i, 0] = elementsPositions[i, 0];
                enlargedLengthCoordinates[i, 1] = elementsPositions[i, 1];
            }

            return enlargedLengthCoordinates;
        }

        static int[,] ReduceLength(int[,] elementsPositions)
        {
            int[,] reducedCoordinates = new int[elementsPositions.GetLength(0) - 1, 2];

            for (int i = 0; i < reducedCoordinates.GetLength(0) - 1; i++)
            {
                reducedCoordinates[i, 0] = elementsPositions[i + 2, 0]; //почему i + 2?
                reducedCoordinates[i, 1] = elementsPositions[i + 2, 1];
            }

            return reducedCoordinates;
        }

        static bool AvoidSelfCrossing(int[] direction, int[,] elementsPositions)
        {
            int matchesAmount = 0;
            int fullMatch = 2;

            for (int i = 0; i < elementsPositions.GetLength(0); i++)
            {
                for (int j = 0; j < elementsPositions.GetLength(1); j++)
                {
                    if (elementsPositions[i, j] == direction[j])
                    {
                        matchesAmount++;
                    }
                }

                if (matchesAmount == fullMatch)
                    return false;
                else
                    matchesAmount = 0;
            }

            return true;
        }

        static bool CanMove(char[,] map, int[,] elementsPositions, int nextPositionX, int nextPositionY)
        {
            char wall = '#';
            bool isNotWall = map[nextPositionX, nextPositionY] != wall;
            bool isNotSelfCrossing;

            if (elementsPositions.GetLength(0) > 1)
            {
                int[] direction = new int[] { nextPositionX, nextPositionY };
                isNotSelfCrossing = AvoidSelfCrossing(direction, elementsPositions); //не работает //пропускает насквозь
            }
            else
            {
                isNotSelfCrossing = true;
            }

            bool canMove = isNotWall && isNotSelfCrossing;//добавить тупик

            return canMove;
        }

        static int[] GetDirect(ConsoleKeyInfo pressedKey, int[,] elementsPositions, char[,] map, ref bool canMove)
        {
            int lastElement = elementsPositions.GetLength(0) - 1;
            int nextPositionX = elementsPositions[lastElement, 0];
            int nextPositionY = elementsPositions[lastElement, 1];

            int[] direction = { nextPositionX, nextPositionY };

            ConsoleKey moveUp = ConsoleKey.UpArrow;
            ConsoleKey moveDown = ConsoleKey.DownArrow;
            ConsoleKey moveLeft = ConsoleKey.LeftArrow;
            ConsoleKey moveRight = ConsoleKey.RightArrow;

            if (pressedKey.Key == moveUp)
                nextPositionY--;
            if (pressedKey.Key == moveDown)
                nextPositionY++;
            if (pressedKey.Key == moveLeft)
                nextPositionX--;
            if (pressedKey.Key == moveRight)
                nextPositionX++;

            canMove = CanMove(map, elementsPositions, nextPositionX, nextPositionY);

            if (canMove)
                direction = new int[] { nextPositionX, nextPositionY };

            return direction;
        }

        static int[,] GetNextPositions(int[,] elementsPositions, int[] direction, bool isDirectionClear)
        {
            if (isDirectionClear)
            {
                if (elementsPositions.GetLength(0) > 1)
                {
                    for (int i = 0; i < elementsPositions.GetLength(0) - 1; i++) //дубляж в EnlargeLength????
                    {
                        elementsPositions[i, 0] = elementsPositions[i + 1, 0];
                        elementsPositions[i, 1] = elementsPositions[i + 1, 1];
                    }
                }
            }
            //if (isDirectionClear) нужна?
            elementsPositions[elementsPositions.GetLength(0) - 1, 0] = direction[0];
            elementsPositions[elementsPositions.GetLength(0) - 1, 1] = direction[1]; //ошибка. Y вне массива? //по-прежнему?

            return elementsPositions;
        }

        static bool CompareCoordinates(HashSet<(int, int)> enlagersCoordinates, HashSet<(int, int)> reducersCoordinates, int[] direction, ref int[,] elementsPositions, ref bool isLastMove) //не матчится. Похоже здесь основная проблема  //EnlargeLength и ReduceLength в отдельный метод?
        {
            HashSet<(int, int)> hashOfDirection = new HashSet<(int, int)> { (direction[0], direction[1]) };

            int enlagersCount = enlagersCoordinates.Count;
            int reducersCount = reducersCoordinates.Count;


            foreach ((int, int) coordinate in enlagersCoordinates)
            {
                if (hashOfDirection.Contains(coordinate))
                {
                    elementsPositions = EnlargeLength(elementsPositions);
                    enlagersCoordinates.Remove(coordinate);
                    return false;
                }
            }

            foreach ((int, int) coordinate in reducersCoordinates)
            {
                if (hashOfDirection.Contains(coordinate))
                {
                    elementsPositions = ReduceLength(elementsPositions);
                    reducersCoordinates.Remove(coordinate);
                    return false;
                }
            }

            isLastMove = elementsPositions.GetLength(0) == 1 && reducersCount != reducersCoordinates.Count; //перенести в Main

            bool areAllEnlagersAlive = enlagersCount == enlagersCoordinates.Count;
            bool areAllReducersAlive = reducersCount == reducersCoordinates.Count;

            return areAllEnlagersAlive || areAllReducersAlive;
        }

        static int[,] GetStartPosition(char[,] map, Random random)
        {
            int minimumPositionX = 1;
            int minimumPositionY = 1;
            int maximumPositionX = map.GetLength(0) - 1;
            int maximumPositionY = map.GetLength(1) - 1;
            int positionX;
            int positionY;
            char freeSpace = ' ';

            do
            {
                positionX = GetRandomCoordinate(minimumPositionX, maximumPositionX, random);
                positionY = GetRandomCoordinate(minimumPositionY, maximumPositionY, random);
            } while (map[positionX, positionY] != freeSpace);

            int[,] startPosition = new int[,] { { positionX, positionY } };

            return startPosition;
        }

        static int GetRandomCoordinate(int minimumCoordinate, int maximumCoordinate, Random random)
        {
            int randomCoordinate = random.Next(minimumCoordinate, maximumCoordinate);

            return randomCoordinate;
        }

        static HashSet<(int, int)> GetWallCoordinates(char[,] map)
        {
            HashSet<(int, int)> wallCoordinates = new HashSet<(int, int)>();

            for (int y = 0; y < map.GetLength(1); y++)
                for (int x = 0; x < map.GetLength(0); x++)
                    if (map[x, y] == '#')
                        wallCoordinates.Add((x, y));

            return wallCoordinates;
        }

        static HashSet<(int, int)> SetUniqueCoordinates(ref HashSet<(int, int)> usedCoordinates, int[] direction, int coordinatesAmount, char[,] map, Random random)
        {
            int minimumPositionX = 1;
            int minimumPositionY = 1;
            int maximumPositionX = map.GetLength(0) - 1;
            int maximumPositionY = map.GetLength(1) - 1;
            int positionX;
            int positionY;

            HashSet<(int, int)> uniqueCoordinates = new HashSet<(int, int)>();
            HashSet<(int, int)> directionCoordinates = new HashSet<(int, int)>() { ((direction[0], direction[1])) };

            for (int i = 0; i < coordinatesAmount; i++)
            {
                do
                {
                    positionX = GetRandomCoordinate(minimumPositionX, maximumPositionX, random);
                    positionY = GetRandomCoordinate(minimumPositionY, maximumPositionY, random);
                } while (usedCoordinates.Contains((positionX, positionY)) || directionCoordinates.Contains((positionX, positionY)));

                uniqueCoordinates.Add((positionX, positionY));
            }

            return uniqueCoordinates;
        }

        static void Renderer(char[,] map, int[,] elementsPositions, HashSet<(int, int)> enlagersCoordinates, HashSet<(int, int)> reducersCoordinates)
        {
            DrawWalls(map);
            DrawAllSigns(enlagersCoordinates, reducersCoordinates);
            DrawSnake(elementsPositions);
        }

        static void DrawWalls(char[,] map)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    Console.Write(map[x, y]);
                }

                Console.Write("\n");
            }
        }

        static void DrawAllSigns(HashSet<(int, int)> enlagersCoordinates, HashSet<(int, int)> reducersCoordinates)
        {
            char enlargerSign = '+';
            char reducerSign = '-';

            foreach ((int, int) signCoordinate in enlagersCoordinates)
                DrawSign(signCoordinate, enlargerSign);

            foreach ((int, int) signCoordinate in reducersCoordinates)
                DrawSign(signCoordinate, reducerSign);
        }

        static void DrawSign((int, int) signCoordinate, char sign)
        {
            Console.SetCursorPosition(signCoordinate.Item1, signCoordinate.Item2);
            Console.Write(sign);
        }

        static void DrawSnake(int[,] elementsPositions)
        {
            char snakeSymbol = 'o';
            ConsoleColor headColor = ConsoleColor.Red;
            ConsoleColor bodyColor = ConsoleColor.Green;
            ConsoleColor defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = headColor;
            int snakeHeadPositionX = elementsPositions[elementsPositions.GetLength(0) - 1, 0];
            int snakeHeadPositionY = elementsPositions[elementsPositions.GetLength(0) - 1, 1];
            Console.SetCursorPosition(snakeHeadPositionX, snakeHeadPositionY);
            Console.Write(snakeSymbol);

            Console.ForegroundColor = defaultColor;

            if (elementsPositions.GetLength(0) > 1)
            {
                Console.ForegroundColor = bodyColor;

                for (int i = elementsPositions.GetLength(0) - 2; i >= 0; i--)
                {
                    Console.SetCursorPosition(elementsPositions[i, 0], elementsPositions[i, 1]);
                    Console.Write(snakeSymbol);
                }

                Console.ForegroundColor = defaultColor;
            }
        }

        static bool CanPlay(int[] direction, ref int[,] elementsPositions, bool isWork, ref bool canMove, HashSet<(int, int)> enlagersCoordinates, HashSet<(int, int)> reducersCoordinates, ref bool isLastMove) //нужен рефакторинг - запрос нового направления не относится к функции верификации возможности продолжения игры
        {
            //isLastMove = elementsPositions.GetLength(0) == 1 && reducersCoordinates.Count != reducersCoordinates.Count;
            if (isLastMove)
                isWork = false;

            return isWork;
        }

        static void ShowInfo(ConsoleKeyInfo pressedKey, int[] direction, int[,] elementsPositions, HashSet<(int, int)> enlagersCoordinates, HashSet<(int, int)> reducersCoordinates)
        {
            Console.SetCursorPosition(0, 11);
            Console.Write("pressedKey - " + pressedKey.Key + "\n\n" +
                "direction X - " + direction[0] + "\n" +
                "direction Y - " + direction[1] + "\n\n" +
                "elements amount - " + elementsPositions.GetLength(0) + "\n\n");

            for (int i = 0; i < elementsPositions.GetLength(0); i++)
            {
                int shiftIndex = 1;
                int nextElement = elementsPositions.GetLength(0) - i - shiftIndex;
                Console.Write($"element {i + 1} coordinate X - " + elementsPositions[nextElement, 0] + "\n" +
                              $"element {i + 1} coordinate Y - " + elementsPositions[nextElement, 1] + "\n\n");
            }

            Console.SetCursorPosition(35, 11);
            Console.Write($"enlagers remained - {enlagersCoordinates.Count}");
            Console.SetCursorPosition(35, 12);
            Console.Write($"reducers remained - {reducersCoordinates.Count}");
        }
    }
}