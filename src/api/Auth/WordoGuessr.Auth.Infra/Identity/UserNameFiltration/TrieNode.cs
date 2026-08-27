namespace WordoGuessr.Auth.Infra.Identity.UserNameFiltration;

internal enum TryMoveResult
{
    UnsafeWordFound,
    BrokenChain,
    Moved
}

internal sealed class TrieNode
{
    public Dictionary<char, TrieNode> ChildNodes { get; } = new Dictionary<char, TrieNode>();
    public bool IsTerminal { get; set; }

    public TryMoveResult TryMove(string word, out TrieNode? newNode)
    {
        var node = this;

        foreach (var character in word)
        {
            if (!node.ChildNodes.TryGetValue(character, out var nextNode))
            {
                newNode = null;
                return TryMoveResult.BrokenChain;
            }

            if (nextNode.IsTerminal)
            {
                newNode = null;
                return TryMoveResult.UnsafeWordFound;
            }

            node = nextNode;
        }

        newNode = node;
        return TryMoveResult.Moved;
    }
}