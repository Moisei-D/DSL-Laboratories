using System;
using System.Collections.Generic;
using System.Text;

namespace Lab6
{
    // ─────────────────────────────────────────────
    //  Base AST node
    // ─────────────────────────────────────────────
    public abstract class AstNode
    {
        /// <summary>Pretty-print the subtree with a given indent level.</summary>
        public abstract string ToTreeString(int indent = 0);

        protected static string Pad(int indent) => new string(' ', indent * 2);
    }

    // ─────────────────────────────────────────────
    //  Expressions
    // ─────────────────────────────────────────────

    /// <summary>A numeric literal, e.g. 3.1415</summary>
    public class NumberLiteralNode : AstNode
    {
        public double Value { get; }
        public NumberLiteralNode(double value) => Value = value;

        public override string ToTreeString(int indent = 0) =>
            $"{Pad(indent)}NumberLiteral({Value})";
    }

    /// <summary>An identifier reference, e.g. x or pi</summary>
    public class IdentifierNode : AstNode
    {
        public string Name { get; }
        public IdentifierNode(string name) => Name = name;

        public override string ToTreeString(int indent = 0) =>
            $"{Pad(indent)}Identifier({Name})";
    }

    /// <summary>A binary operation, e.g. a + b</summary>
    public class BinaryExpressionNode : AstNode
    {
        public string Operator { get; }
        public AstNode Left { get; }
        public AstNode Right { get; }

        public BinaryExpressionNode(string op, AstNode left, AstNode right)
        {
            Operator = op;
            Left = left;
            Right = right;
        }

        public override string ToTreeString(int indent = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Pad(indent)}BinaryExpression({Operator})");
            sb.AppendLine($"{Pad(indent + 1)}left:");
            sb.AppendLine(Left.ToTreeString(indent + 2));
            sb.Append($"{Pad(indent + 1)}right:");
            sb.AppendLine();
            sb.Append(Right.ToTreeString(indent + 2));
            return sb.ToString().TrimEnd();
        }
    }

    /// <summary>A function call, e.g. sin(pi)</summary>
    public class FunctionCallNode : AstNode
    {
        public string FunctionName { get; }
        public List<AstNode> Arguments { get; }

        public FunctionCallNode(string name, List<AstNode> args)
        {
            FunctionName = name;
            Arguments = args;
        }

        public override string ToTreeString(int indent = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Pad(indent)}FunctionCall({FunctionName})");
            for (int i = 0; i < Arguments.Count; i++)
            {
                sb.AppendLine($"{Pad(indent + 1)}arg[{i}]:");
                if (i < Arguments.Count - 1)
                    sb.AppendLine(Arguments[i].ToTreeString(indent + 2));
                else
                    sb.Append(Arguments[i].ToTreeString(indent + 2));
            }
            return sb.ToString().TrimEnd();
        }
    }

    // ─────────────────────────────────────────────
    //  Statements
    // ─────────────────────────────────────────────

    /// <summary>A let declaration, e.g. let x = 10;</summary>
    public class LetStatementNode : AstNode
    {
        public string Name { get; }
        public AstNode Value { get; }

        public LetStatementNode(string name, AstNode value)
        {
            Name = name;
            Value = value;
        }

        public override string ToTreeString(int indent = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Pad(indent)}LetStatement");
            sb.AppendLine($"{Pad(indent + 1)}name: {Name}");
            sb.AppendLine($"{Pad(indent + 1)}value:");
            sb.Append(Value.ToTreeString(indent + 2));
            return sb.ToString().TrimEnd();
        }
    }

    // ─────────────────────────────────────────────
    //  Program (root node)
    // ─────────────────────────────────────────────

    /// <summary>The root of the AST — a list of top-level statements.</summary>
    public class ProgramNode : AstNode
    {
        public List<AstNode> Statements { get; } = new List<AstNode>();

        public override string ToTreeString(int indent = 0)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Pad(indent)}Program");
            for (int i = 0; i < Statements.Count; i++)
            {
                sb.AppendLine($"{Pad(indent + 1)}statement[{i}]:");
                if (i < Statements.Count - 1)
                    sb.AppendLine(Statements[i].ToTreeString(indent + 2));
                else
                    sb.Append(Statements[i].ToTreeString(indent + 2));
            }
            return sb.ToString().TrimEnd();
        }
    }
}