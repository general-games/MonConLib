namespace MonCon
{
    public class ConsoleCommander
    {
        private ConsoleCommandNode _root;
        public ConsoleCommandNode Root => _root;

        public ConsoleCommander()
        {
            _root = new ConsoleCommandNode("root");
        }

        public void AddNode(ConsoleCommandNode node)
        {
            if(node == _root)
                return;

            _root.AddNode(node);
        }

        public void RemoveNode(string nodeName)
        {
            if (nodeName == "root")
                return;


            var child = _root.GetNode(nodeName);
            if (child != null)
            {
                _root.Nodes.Remove(nodeName.ToLower());
            }
        }
    }
}
