using System;
using System.Collections.Generic;

namespace MonCon
{
    public class ConsoleCommandNode
    {
        public string Name { get; }
        public string Value { get; set; }
        public bool IsParameter { get; }
        public Dictionary<string, ConsoleCommandNode> Nodes { get; } = new();
        public Action<string> OnSetValue { get; set; }
        public List<string> Parameters { get; set; } = new();

        public ConsoleCommandNode(string name, bool isParameter = false)
        {
            Name = name;
            IsParameter = isParameter;
        }

        public void AddNode(ConsoleCommandNode child)
        {
            Nodes[child.Name.ToLower()] = child;
        }

        public ConsoleCommandNode GetNode(string name)
        {
            Nodes.TryGetValue(name.ToLower(), out var child);
            return child;
        }

        public IEnumerable<string> ListChildren() => Nodes.Keys;
    }

    public class ToggleParameterNode : ConsoleCommandNode
    {
        public ToggleParameterNode(string name, string initialValue, Action onCallback, Action offCallback) : base(name, isParameter: true)
        {
            Value = initialValue;
            Parameters = new List<string> { "on", "off" };
            OnSetValue = (val) =>
            {
                if (val == "on")
                {
                    onCallback?.Invoke();
                }
                else if (val == "off")
                {
                    offCallback?.Invoke();
                }
                Value = val;
            };
        }
    }

    public class NumbersParameterNode : ConsoleCommandNode
    {
        public NumbersParameterNode(string name, string initialValue, Action<int> onSetCallback, int length) : base(name, isParameter: true)
        {
            int counter = 1;
            for (int i = 0; i < length; i++)
            {
                Parameters.Add($"{counter++}");
            }

            OnSetValue = (val) =>
            {
                int number = int.TryParse(val, out int result) ? result : 0;
                onSetCallback?.Invoke(result);
                Value = val;
            };

        }
    }

    public class AnyNumbersParameterNode : ConsoleCommandNode
    {
        public AnyNumbersParameterNode(string name, string initialValue, Action<int> onSetCallback) : base(name, isParameter: true)
        {
            Parameters = new List<string> { "Enter any number" };
            OnSetValue = (val) =>
            {
                if (int.TryParse(val, out int number))
                {
                    onSetCallback?.Invoke(number);
                    Value = val;
                }
            };
        }
    }

    public class NumbersPairParameterNode : ConsoleCommandNode
    {
        public NumbersPairParameterNode(string name, (int x, int y) pairs, Action<int, int> onSetCallback) : base(name, isParameter: true)
        {
            Parameters = new List<string> { $"{pairs.x},{pairs.y}" };
            OnSetValue = (val) =>
            {
                var parts = val.Split(',');
                if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                {
                    onSetCallback?.Invoke(x, y);
                    Value = val;
                }
            };
        }
    }

    public class StringParameterNode : ConsoleCommandNode
    {
        public StringParameterNode(string name, List<string> parameters, Action<string> onSetCallback) : base(name, isParameter: true)
        {
            Parameters = parameters;
            Value = "undefined";
            OnSetValue = (val) =>
            {
                onSetCallback?.Invoke(val);
                Value = val;
            };
        }
    }

    public class MethodParameterNode : ConsoleCommandNode
    {
        public MethodParameterNode(string name, ConsoleCommandSignature commandSig) : base(name, isParameter: true)
        {
            Parameters = new List<string> { $"Use | ->{commandSig.Signature} | to invoke: {name}" };
            OnSetValue = (val) =>
            {
                if (commandSig.Signature == val)
                    commandSig.Callback?.Invoke();
                Value = "";
            };
        }
    }


    public class ConsoleCommandSignature
    {
        public string Signature { get; set; }
        public Action Callback { get; set; }

        public ConsoleCommandSignature(string signature, Action callback)
        {
            Signature = signature;
            Callback = callback;
        }
    }

}
