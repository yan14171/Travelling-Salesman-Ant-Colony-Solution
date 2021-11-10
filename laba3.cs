
#region prepare
int iterCount = 100,
	antCount  = 30,
	citiesCount = 100,
	Lmin,
	antPosition = 0,
	newAntPosition;
	
int[] Lk = new int[antCount];
var positionRandomizer = new Random();
var visited = new HashSet<int>();
var distances = getDistances(citiesCount);
var pheromone = new double[citiesCount, citiesCount];

for (int i = 0; i < pheromone.GetLength(0); i++)
	for(int j = 0; j < pheromone.GetLength(1); j++)
		pheromone[i,j] = 1;
		
Lmin = greedyLength(0, distances);
var memory = new bool[antCount, citiesCount, citiesCount];
int[] curShortestPheromonePath;
int Lpr;
#endregion


for (int i = 0; i < iterCount; i++)
{
	for (int j = 0; j < antCount; j++)
	{
		antPosition = positionRandomizer.Next(0, citiesCount); // generate an initial position for an ant
		visited.Add(antPosition); // add initial state as visited
		while (visited.Count != citiesCount) // while all cities haven't been visited
		{
			newAntPosition = move(distances, antPosition); // calculate a position, where the ant will move
			Lk[j] += distances[antPosition, newAntPosition]; // add the move distance to the ant's already traveled distance
			memory[j, antPosition, newAntPosition] = true; // remember, that this ant j has visited the pass from [antPosition] to [newAntPosition]
			antPosition = newAntPosition; // move the ant
			visited.Add(antPosition);   // set the new position as already seen
		}
		visited.Clear(); // clear the set for the new ant
	}
	update(pheromone); // update the level of pheromones on the map
	curShortestPheromonePath = getShortestPheromonePath(pheromone);
	Lpr = getLpr(curShortestPheromonePath);
	if (Lpr < Lmin)
		Lmin = Lpr;
	visited.Clear();
	if (i % 20 == 0)
	{

		$"Iteration {i} ------".Dump();
		$"Lpr = {Lpr}".Dump();
		pheromone.Dump();
		$"Lmin = {Lmin}".Dump();
	}
}
Lmin.Dump();
greedyLength(0,distances).Dump();

int getLpr(int[] citiesMap)
{
	int num = 0;
	for (int i = 0; i < (citiesMap.Length - 1); i++)
	{
		num += distances[citiesMap[i], citiesMap[i+1]];
	}
	return num;
}

int[] getShortestPheromonePath(double[,] pheromone) {
	var resultPath = new Dictionary<int, double>(); // index / pheromone
	int resultLength = 0,
		curNode = 0;
		resultPath.Add(0,int.MaxValue);
	while (resultPath.Count < pheromone.GetLength(0))
	{
		resultPath = resultPath
		.Concat(
			Enumerable.Range(0, pheromone.GetUpperBound(1) + 1)
			.Select(i => KeyValuePair.Create(i,pheromone[curNode, i]))
		.OrderByDescending(n => resultPath.ContainsKey(n.Key) ? .0 : n.Value))
		.Take(++resultLength)
		.ToDictionary(n => n.Key, n => n.Value);
		
		curNode = resultPath.Last().Key;
	}
	return resultPath.Keys.ToArray();
}

void update(double[,] pheromone){
	double p = 0.4;
	for (int i = 0; i < pheromone.GetLength(0); i++)
	{
		for (int j = 0; j < pheromone.GetLength(1); j++)
		{
			pheromone[i,j] *= p;
			pheromone[i,j] += calculateDeltaPheromone(i, j);
		}
		pheromone[i,i] = 1;
	}
}

double calculateDeltaPheromone(int from, int to)
{
	double num = 0.0;
	for (int i = 0; i < antCount; i++)
	{
		bool flag = memory[i, from, to];
		num = flag ? (num + (((double)Lmin) / ((double)Lk[i]))) : (num + 0.0);
	}
	return num;
}

int move(byte[,] distances, int prevPos)
{
	int alpha = 2,
		beta = 4;

	var movingRandomizer = new Random();
	byte[] availablePasses =
	Enumerable.Range(0, distances.GetUpperBound(1) + 1)
										.Select(i => distances[prevPos, i])
										.ToArray();
	double fullProbability = 0;
	double pherToDistRelation;
	double sum = 0;
	for (int i = 0; i < availablePasses.Length; i++)
	{
		if (!visited.Contains(i))
		{
			pherToDistRelation = Math.Pow(pheromone[prevPos, i], alpha) * Math.Pow((double)1 / availablePasses[i], beta);
			fullProbability += pherToDistRelation;
		}
	}

	double[] availableProbabilities = availablePasses.Select((n, index) =>
	{
		if (visited.Contains(index))
			return 0;

		pherToDistRelation = Math.Pow(pheromone[prevPos, index], alpha) * Math.Pow((double)1 / n, beta);

		double prob = pherToDistRelation / fullProbability;
		sum += prob;
		return prob;
	}).ToArray();

	double sum1 = 0;
	double randomNum = movingRandomizer.NextDouble();
	for (int i = 0; i < availableProbabilities.Length; i++)
	{
		sum1 += availableProbabilities[i];
		if (sum1 >= randomNum) return i;
	}
	throw new Exception("wut");
}

int greedyLength(int startingIndex, byte[,] graph){
 	int length = 0;
	int minLengthIndex = 0;
	HashSet<int> visited = new HashSet<int>() {startingIndex};
	while(visited.Count != graph.GetLength(0))
	{
		for (int i = 0; i < graph.GetLength(1); i++)
		{
			if(graph[startingIndex, i] < graph[startingIndex, minLengthIndex] && !visited.Contains(i))
				minLengthIndex = i;
		}
		visited.Add(minLengthIndex);
		length += graph[startingIndex,minLengthIndex];
		startingIndex = minLengthIndex;
	}
	
	return length;
}

byte[,] getDistances(int length)
{
	var r = new Random();
	var distances = new byte[length,length];
	for (int i = 0; i < length; i++)
	{
		for (int j = 0; j < i; j++)
		{
			distances[i, j] = (byte)r.Next(5, 51);
			distances[j, i] = distances[i, j];
		}
		
		distances[i, i] = byte.MaxValue;
	}
	
	return distances;
}