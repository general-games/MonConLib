using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonCon
{
    public class ConsoleHandler
    {
        private bool _isOpen = false;
        private StringBuilder _inputBuffer = new();
        private List<string> _outputLog = new();
        private int _scrollOffset = 0;
        private Keys _submitKey = Keys.Enter;
        private KeyboardState _prevKeyboardState;
        private SpriteFont _font;
        private int _consoleHeight;
        private int _consoleWidth;
        private int _lineHeight;
        private ConsoleCommandNode _commandTree;
        private ConsoleLogger _logger;
        private GameWindow _window;

        public bool IsOpen => _isOpen;

        public ConsoleHandler(ConsoleLogger consoleLogger, ConsoleCommander consoleCommander)
        {
            _logger = consoleLogger;
            _commandTree = consoleCommander.Root;
        }

        public void AddNode(ConsoleCommandNode node)
        {
            _commandTree.AddNode(node);
        }

        public void AttachToWindow(GameWindow window)
        {
            if (_window == window)
                return;

            if (_window != null)
                _window.TextInput -= OnTextInput;

            _window = window;

            if (_window != null)
                _window.TextInput += OnTextInput;
        }

        private void OnTextInput(object? sender, TextInputEventArgs e)
        {
            if (!_isOpen)
                return;

            if (e.Key == _submitKey || e.Character == '\r' || e.Character == '\n')
            {
                SubmitInput();
                return;
            }

            if (e.Key == Keys.Back)
            {
                if (_inputBuffer.Length > 0)
                    _inputBuffer.Length--;

                return;
            }

            if (e.Key == Keys.Delete)
            {
                _inputBuffer.Clear();
                return;
            }

            if (!char.IsControl(e.Character) && e.Character != '\0')
            {
                _inputBuffer.Append(e.Character);
            }
        }



        private void SubmitInput()
        {
            string input = _inputBuffer.ToString();
            if (!string.IsNullOrWhiteSpace(input))
            {
                _outputLog.Add($"> {input}");
                _scrollOffset = 0;
                if (_outputLog.Count > 1000)
                    _outputLog.RemoveAt(0);

                if (input.StartsWith(":"))
                    ProcessConsoleCommand(input);

                if (!input.EndsWith(":"))
                    _inputBuffer.Clear();
            }
        }


        public void LoadContent(SpriteFont font)
        {
            _font = font;
            _lineHeight = (int)_font.MeasureString("A").Y + 2;
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (_isOpen)
            {
                if (!_prevKeyboardState.IsKeyDown(Keys.PageUp) && keyboardState.IsKeyDown(Keys.PageUp))
                {
                    int visibleLines = GetVisibleLines();
                    int maxScroll = Math.Max(0, _logger.Logs.Count + _outputLog.Count - visibleLines);
                    _scrollOffset = Math.Min(_scrollOffset + 3, maxScroll);
                }
                if (!_prevKeyboardState.IsKeyDown(Keys.PageDown) && keyboardState.IsKeyDown(Keys.PageDown))
                {
                    _scrollOffset = Math.Max(_scrollOffset - 3, 0);
                }
            }

            _prevKeyboardState = keyboardState;
        }

        public void ToggleConsole()
        {
            _isOpen = !_isOpen;
        }

        private void ProcessConsoleCommand(string input)
        {
            if (!input.StartsWith(":"))
                return;

            string pathPart = input;
            string valuePart = null;
            if (input.Contains("->"))
            {
                var split = input.Split(["->"], StringSplitOptions.None);
                pathPart = split[0];
                if (split.Length > 1)
                    valuePart = split[1].Trim();
            }

            var segments = pathPart.Split(':', StringSplitOptions.RemoveEmptyEntries);
            var node = _commandTree;
            int i = 0;
            while (i < segments.Length)
            {
                var seg = segments[i];
                if (seg.StartsWith(":"))
                    seg = seg.TrimStart(':');
                if (seg == "")
                {
                    i++;
                    continue;
                }
                var child = node.GetNode(seg);
                if (child == null)
                {
                    WriteLine($"Node '{seg}' not found.");
                    return;
                }
                node = child;
                i++;
            }
            if (valuePart != null)
            {
                if (node.IsParameter)
                {
                    node.Value = valuePart;
                    node.OnSetValue?.Invoke(valuePart);
                    WriteLine($"Set {node.Name} to {valuePart}");
                }
                else
                {
                    WriteLine($"Node '{node.Name}' is not a parameter.");
                }
            }
            else
            {
                if (node.Nodes.Count > 0)
                {
                    WriteLine($"{node.Name}: {string.Join(", ", node.ListChildren())}");
                }
                else if (node.IsParameter)
                {
                    WriteLine($"{node.Name} = {node.Value}");
                    if (node.Parameters.Count > 0)
                    {
                        WriteLine("Paramters: - " + string.Join("  - ", node.Parameters));
                    }
                }
            }
        }

        private void DrawConsoleBackground(SpriteBatch spriteBatch)
        {
            _consoleWidth = spriteBatch.GraphicsDevice.Viewport.Width;
            _consoleHeight = spriteBatch.GraphicsDevice.Viewport.Height / 2;

            Texture2D rect = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            rect.SetData(new[] { new Color(0, 0, 0, 200) });
            spriteBatch.Draw(rect, new Rectangle(0, 0, _consoleWidth, _consoleHeight), Color.White);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_isOpen || _font == null)
                return;


            DrawConsoleBackground(spriteBatch);


            List<string> combined = new List<string>(_logger.Logs.Count + _outputLog.Count);
            combined.AddRange(_logger.Logs);
            combined.AddRange(_outputLog);

            int visibleLines = GetVisibleLines();
            int totalLines = combined.Count;
            int startLine = Math.Max(0, totalLines - visibleLines - _scrollOffset);
            int endLine = Math.Min(totalLines, startLine + visibleLines);

            int y = 5;
            for (int i = startLine; i < endLine; i++)
            {
                var line = combined[i];
                if (line.Contains(":") && !line.StartsWith("> ") && !line.Contains("not found") && !line.Contains("is not a parameter") && !line.Contains("Set "))
                {
                    int colonIdx = line.IndexOf(":");
                    if (colonIdx > 0)
                    {
                        string nodeName = line.Substring(0, colonIdx);
                        string childrenList = line.Substring(colonIdx + 1).Trim();
                        spriteBatch.DrawString(_font, nodeName, new Vector2(10, y), Color.White);
                        float x = 10 + _font.MeasureString(nodeName).X;
                        spriteBatch.DrawString(_font, ": ", new Vector2(x, y), Color.White);
                        x += _font.MeasureString(": ").X;
                        var children = childrenList.Split(",");
                        for (int j = 0; j < children.Length; j++)
                        {
                            string child = children[j].Trim();
                            var parentNode = _commandTree.GetNode(nodeName);
                            Color color = Color.White;
                            if (parentNode != null)
                            {
                                var childNode = parentNode.GetNode(child);
                                if (childNode != null && childNode.IsParameter)
                                    color = Color.HotPink;
                            }
                            spriteBatch.DrawString(_font, child, new Vector2(x, y), color);
                            x += _font.MeasureString(child).X;
                            if (j < children.Length - 1)
                            {
                                spriteBatch.DrawString(_font, ", ", new Vector2(x, y), Color.Lime);
                                x += _font.MeasureString(", ").X;
                            }
                        }
                        y += _lineHeight;
                        continue;
                    }
                }
                spriteBatch.DrawString(_font, line, new Vector2(10, y), Color.Lime);
                y += _lineHeight;
            }

            spriteBatch.DrawString(_font, "> " + _inputBuffer.ToString(), new Vector2(10, _consoleHeight - _lineHeight - 5), Color.Yellow);
        }

        private int GetVisibleLines()
        {
            return Math.Max(1, (_consoleHeight - _lineHeight - 10) / _lineHeight);
        }

        public void WriteLine(string text)
        {
            _outputLog.Add(text);
            _scrollOffset = 0;

            if (_outputLog.Count > 1000)
                _outputLog.RemoveAt(0);
        }
    }
}
