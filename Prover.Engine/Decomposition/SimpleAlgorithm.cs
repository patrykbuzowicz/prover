﻿using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Prover.Engine.Types.Decomposition;
using Prover.Engine.Types.Expression;

namespace Prover.Engine.Decomposition
{
    internal class SimpleAlgorithm : Algorithm
    {
        public override AlgorithmResult Solve(IExpression rootExpression)
        {
            List<INode> nodes = new List<INode>();
            List<IConnection> connections = new List<IConnection>();
            Stack<INode> toDecompose = new Stack<INode>();

            INode rootNode = new SimpleNode(null, new Negation(rootExpression));
            toDecompose.Push(rootNode);
            nodes.Add(rootNode);

            ((Node)rootNode).IsBranchClosed = true;

            while (toDecompose.Count > 0)
            {
                INode decomposedNode = toDecompose.Pop();
                IExpression expression = decomposedNode.GetExpression();
                DecompositionResult decompositionResult = expression.Decompose();

                IEnumerable<INode> decomposedNodes;

                if (decompositionResult.Type == DecompositionType.Beta)
                {
                    decomposedNodes = decomposedNode.CreateBranch(decompositionResult.LeftExpression, decompositionResult.RightExpression);
                }
                else
                {
                    decomposedNodes = new List<INode>
                    {
                        decomposedNode.CreateNode(decompositionResult.LeftExpression,
                            decompositionResult.RightExpression)
                    };
                }

                connections.AddRange(decomposedNode.Children);

                foreach (INode node in decomposedNodes)
                {
                    if (node.CanDecompose)
                    {
                        toDecompose.Push(node);
                    }

                    ((Node)node).IsBranchClosed = true;
                    nodes.Add(node);
                }
            }

            IEnumerable<INode> openNodes = nodes.Where(x => !x.HasNonLiterals && !x.IsClosed);

            foreach (INode openNode in openNodes)
            {
                MarkNodeAndParentsOpen((Node) openNode);
            }

            return new AlgorithmResult
            {
                Connections = connections,
                Nodes = nodes,
                IsTautology = !openNodes.Any()
            };
        }
    }
}