using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.CSharp;

namespace ICSharpCode.Decompiler.Ast.Transforms
{
	class FlattenElseIfStatements : IAstTransform
	{
		public void Run(AstNode compilationUnit)
		{
			foreach (var ifElseStatement in compilationUnit.Descendants.OfType<IfElseStatement>())
			{
				if (!(ifElseStatement.FalseStatement is BlockStatement))
					continue;
				if (!(ifElseStatement.FalseStatement.FirstChild is IfElseStatement))
					continue;
				if (ifElseStatement.FalseStatement.Children.Count() != 1)
					continue;

				ifElseStatement.FalseStatement.ReplaceWith(ifElseStatement.FalseStatement.FirstChild);
			}
		}
	}
}
