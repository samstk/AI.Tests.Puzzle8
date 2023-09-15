// See https://aka.ms/new-console-template for more information

// Define the problem space

using SSDK.AI;
using SSDK.AI.Solvers;
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
        4, 3, 0, 5, 8, 7, 2, 6, 1
        );

        // Guided problem solving
        Puzzle8Problem[] steps = new Puzzle8Problem[]
        {
            new Puzzle8Problem(true,
                1, 2, 3,
                7, -1, -1,
                -1, -1, -1
                ),
            new Puzzle8Problem(true,
                1, 2, 3,
                7, 0, 6,
                -1, -1, -1
                ),
            new Puzzle8Problem(true,
                1, 2, 3,
                0, 5, 7,
                -1, -1, -1
                ),
            new Puzzle8Problem(true,
                1, 2, 3,
                4, 5, 6,
                -1, -1, -1
                ),
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
            ), initialProblem, new BFSSolver());
        
        agent.Guide(steps);

        AgentOperation completeOperation = new AgentOperation();

        DateTime start = DateTime.Now;
        Console.WriteLine("Solving...");
        for (int i = 0; i < steps.Length; i++)
        {
            
            
            agent.Solve();
            completeOperation.Merge(agent.CurrentOperation);
            agent.ExecuteAll(); // Execute all actions to solve sub-problem.
            
        }
        Console.WriteLine($"Solved in {(DateTime.Now - start)}");
        Console.WriteLine(completeOperation.ToString().Replace("(1)", "UP").Replace("(2)", "DOWN").Replace("(3)", "LEFT").Replace("(4)", "RIGHT"));
        Console.WriteLine("RESULTING IN");
        Console.WriteLine(agent.CurrentProblemSpace);
    }
}

