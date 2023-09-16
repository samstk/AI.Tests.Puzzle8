// See https://aka.ms/new-console-template for more information

// Define the problem space

using SSDK.AI;
using SSDK.AI.Agent;
using SSDK.AI.Agent.Solvers;
using SSDK.AI.KBS;
using SSDK.AI.KBS.Logic;
using SSDK.Core.Structures.Primitive;
using static Puzzle8Problem;

/// <summary>
/// Indicates to a state, that another state is closer to this regardless 
/// of what the given indices have.
/// </summary>
public sealed class Wildcard
{
    /// <summary>
    /// The list of indices where the wildcard is applied
    /// </summary>
    public List<int> Indices = new List<int>();
}

public class Puzzle8Problem : AgentProblemSpace
{
    public int[] State = new int[9];
    public const int MOVE_UP = 1;
    public const int MOVE_DOWN = 2;
    public const int MOVE_LEFT = 3;
    public const int MOVE_RIGHT = 4;
    public Wildcard Wildcard;
    public Puzzle8Problem(params int[] state)
    {
        if (state.Length != 9) throw new Exception("Invalid state");
        State = state;
    }

    public Puzzle8Problem(bool wildcardIndicator, params int[] state)
    {
        if (state.Length != 9) throw new Exception("Invalid state");
        State = state;
        if(wildcardIndicator)
        for(int i = 0; i<9; i++)
            if (State[i] == -1)
            {
                if (Wildcard == null) Wildcard = new Wildcard();
                Wildcard.Indices.Add(i);
            }
    }
    public override void Perceive(Agent agent)
    {
        // Left as empty function as problem space is entirety of world
    }

    public override AgentProblemSpace Predict(Agent agent, AgentOperation operation)
    {
        int actionType = operation.AsSingle();

        int[] newState = new int[9];
        Array.Copy(State, newState, 9);
        if (actionType == MOVE_LEFT)
        {
            // Move left
            for(int i = 0; i<9; i++)
            {
                int x = i % 3;
                int y = i / 3;

                if (State[i] == 0)
                {
                    if (x == 0) return this; // No changes
                    int swapIndex = (x - 1) + y * 3;
                    newState[i] = State[swapIndex];
                    newState[swapIndex] = 0;
                    break;
                }
            }
            return new Puzzle8Problem(newState);
        }
        else if (actionType == MOVE_RIGHT)
        {
            // Move right
            for (int i = 0; i < 9; i++)
            {
                int x = i % 3;
                int y = i / 3;

                if (State[i] == 0)
                {
                    if (x == 2) return this; // No changes
                    int swapIndex = (x + 1) + y * 3;
                    newState[i] = State[swapIndex];
                    newState[swapIndex] = 0;
                    break;
                }
            }
            return new Puzzle8Problem(newState);
        }
        else if (actionType == MOVE_UP)
        {
            // Move up
            for (int i = 0; i < 9; i++)
            {
                int x = i % 3;
                int y = i / 3;

                if (State[i] == 0)
                {
                    if (y == 0) return this; // No changes
                    int swapIndex = (x) + (y - 1) * 3;
                    newState[i] = State[swapIndex];
                    newState[swapIndex] = 0;
                    break;
                }
            }
            return new Puzzle8Problem(newState);
        }
        else if (actionType == MOVE_DOWN)
        {
            // Move down
            for (int i = 0; i < 9; i++)
            {
                int x = i % 3;
                int y = i / 3;

                if (State[i] == 0)
                {
                    if (y == 2) return this; // No changes
                    int swapIndex = (x) + (y + 1) * 3;
                    newState[i] = State[swapIndex];
                    newState[swapIndex] = 0;
                    break;
                }
            }
            return new Puzzle8Problem(newState);
        }
        return this; // Invalid action so just return unchanged problem
    }

    public override double DistanceTo(AgentProblemSpace space)
    {
        // Direct memory reference
        if (space == this) return 0;

        Puzzle8Problem problem = space as Puzzle8Problem;
        int dist = 0;
        for(int i = 0; i<9; i++)
        {
            if (problem.Wildcard != null && problem.Wildcard.Indices.Contains(i)) continue;
            if (State[i] != problem.State[i])
            {
                dist++;
            }
        }
        return dist;
    }

    /// <summary>
    /// Gets the position of the number in the state space.
    /// </summary>
    /// <param name="num">the num to look for</param>
    /// <returns>the position of the num</returns>
    public int PositionOf(int num)
    {
        for (int x = 0; x < 9; x++)
        {
            // Check for position of num
            if (State[x] == num)
            {
                return x;
            }
        }
        
        return -1;
    }
    public override UncontrolledNumber Heuristic(AgentProblemSpace space)
    {
        Puzzle8Problem problem = space as Puzzle8Problem;
        // Commonly used heuristic - manhattan distance for each tile.
        double total = 0;
        for(int i = 0; i<9; i++)
        {
            int pf = PositionOf(i);
            int pfx = pf % 3;
            int pfy = pf / 3;
            
            int pt = problem.PositionOf(i);
            int ptx = pt % 3;
            int pty = pt / 3;

            total += Math.Abs(ptx - pfx) + Math.Abs(pty - pfy);
        }
        return total;
    }

    public override int GetHashCode()
    {
        int hash = 0;
        for(int i = 0; i<9; i++)
        {
            hash += State[i].GetHashCode() * i.GetHashCode();
        }
        return hash;
    }

    public override string ToString()
    {
        string txt = "";
        for(int y = 0; y<3; y++)
        {
            if (y != 0)
                txt += "\n";
            for(int x = 0; x<3; x++)
            {
                if (Wildcard != null && Wildcard.Indices.Contains(x+y*3)) txt += "? ";
                else txt += State[y * 3 + x] + " ";
            }
        }
        return txt;
    }
}

public static class Program
{
    public static void Main()
    {

        Console.WriteLine("Creating inital problem space..");

        Puzzle8Problem initialProblem = new Puzzle8Problem(
        8, 7, 6, 5, 4, 3, 2, 1, 0
        );

        // Guided problem solving
        // Previously, there were several guided steps
        // to prevent BFS and UCS from dropping performance.
        // GBFS is the best solution at this point in time, without 
        // the need for guiding, and generates a more optimal solution.
        // A* costs more performance-wise, and unnecessary as
        // action costs are the same.
        Puzzle8Problem[] steps = new Puzzle8Problem[]
        {
            new Puzzle8Problem(true,
                1, 2, 3,
                4, 5, 6,
                7, 8, 0
                ),
        };

        Console.WriteLine("Creating Agent...");


        Agent agent = new Agent(new AgentActionSpace(
            new AgentAction((a, t) => {
                a.UpdateProblemUsingPrediction(t);
                }, null, MOVE_UP, MOVE_RIGHT) { Name = "MOVE" }
            ), initialProblem, new GBFSSolver());
        
        agent.Guide(steps);

        AgentOperation completeOperation = new AgentOperation();

        DateTime start = DateTime.Now;
        Console.WriteLine($"Solving {agent.Solver}...");
        for (int i = 0; i < steps.Length; i++)
        {
            
            
            agent.Solve();
            completeOperation.Merge(agent.CurrentOperation);
            agent.ExecuteAll(); // Execute all actions to solve sub-problem.
            
        }
        Console.WriteLine($"Solved in {(DateTime.Now - start)}");

        // Animate the response
        foreach(AgentOperationStep step in completeOperation.Steps)
        {
            Thread.Sleep(50);
            Console.Clear();
            Console.WriteLine(step.ToString().Replace("(1)", "UP").Replace("(2)", "DOWN").Replace("(3)", "LEFT").Replace("(4)", "RIGHT"));
            initialProblem = initialProblem.Predict(agent, new AgentOperation(step.AsNew())) as Puzzle8Problem;
            Console.WriteLine(initialProblem);
        }
        Console.WriteLine("FINAL RESULT");
        Console.WriteLine(agent.CurrentProblemSpace);

        Console.WriteLine("ALL STEPS");
        Console.WriteLine(completeOperation.ToString().Replace("(1)", "UP").Replace("(2)", "DOWN").Replace("(3)", "LEFT").Replace("(4)", "RIGHT"));
    }
}