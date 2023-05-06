using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using CodeConverter;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using static CodeConverter.CSharpParser;

namespace CodeConverter
{
	internal class Program
	{
		static void Main(string[] args)
		{
			string code = File.ReadAllText("D:\\Projects\\CodeConverter\\CodeConverter\\code.txt");

			AntlrInputStream inputStream = new AntlrInputStream(code);

			CSharpLexer csLexer = new CSharpLexer(inputStream);

			CommonTokenStream commonTokenStream = new CommonTokenStream(csLexer);
			CSharpParser csParser = new CSharpParser(commonTokenStream);
			CSharpParser.StartContext startContext = csParser.start();
			CSharpVisitor visitor = new CSharpVisitor();
			string pythonCode = visitor.Visit(startContext);
			File.WriteAllText("D:\\Projects\\CodeConverter\\CodeConverter\\code.py", pythonCode);
		}
	}
}

public class CSharpVisitor : CSharpBaseVisitor<string>
{
	int contextLevel = 0;

	private string GetIndent(int contextLevel)
	{
		return string.Join("", Enumerable.Range(0, contextLevel * 4).Select(i => " "));
	}

	public override string VisitStart(StartContext context)
	{
		List<string> python = new();

		foreach (var i in context.children)
			python.Add(Visit(i));

		return string.Join("\r\n", python);
	}

	public override string VisitStatements([Antlr4.Runtime.Misc.NotNull] StatementsContext context)
	{
		List<string> pythonStatements = new();

		foreach (var i in context.children)
			pythonStatements.Add(Visit(i));

		pythonStatements.RemoveAll(s => string.IsNullOrEmpty(s));

		return string.Join("\r\n", pythonStatements);
	}

	public override string VisitStatement(StatementContext context)
	{
		return Visit(context.children[0]);
	}

	public override string VisitDeclaration(DeclarationContext context)
	{
		string type = context.TYPE().GetText();
		string var_name = context.VAR_NAME().GetText();

		//return $"{GetIndent(contextLevel)}{var_name}: {type}";
		return $"{GetIndent(contextLevel)}{var_name}";
	}

	public override string VisitAssignment(AssignmentContext context)
	{
		string left_part;
		if (context.declaration() is not null)
			left_part = Visit(context.declaration());
		else
			 left_part = $"{GetIndent(contextLevel)}{context.VAR_NAME().GetText()}";
		string assignment_operator = context.ASSIGNMENT_OPERATOR().GetText();
		string expression = Visit(context.expression());

		return $"{left_part} {assignment_operator} {expression}";
	}

	public override string VisitExpression(ExpressionContext context)
	{
		List<string> expr = new();

		foreach (var i in context.children)
			expr.Add(Visit(i));

		expr.RemoveAll(s => string.IsNullOrEmpty(s));

		return string.Join(" ", expr);
	}

	public override string VisitOperator(OperatorContext context)
	{
		List<string> expr = new();

		foreach (var i in context.children)
			expr.Add(i.GetText());

		return string.Join(" ", expr);
	}

	public override string VisitPrimaryLiteral(PrimaryLiteralContext context)
	{
		List<string> expr = new();

		foreach (var i in context.children)
			expr.Add(i.GetText());

		expr.RemoveAll(s => string.IsNullOrEmpty(s));

		return string.Join(" ", expr);
	}

	public override string VisitControlConstructs(ControlConstructsContext context)
	{
		List<string> python = new();

		foreach (var i in context.children)
			python.Add(Visit(i));

		python.RemoveAll(s => string.IsNullOrEmpty(s));

		return string.Join("\r\n", python);
	}

	public override string VisitControlConstruct(ControlConstructContext context)
	{
		return Visit(context.children[0]);
	}

	public override string VisitConditional([Antlr4.Runtime.Misc.NotNull] ConditionalContext context)
	{
		string expression = Visit(context.expression());
		++contextLevel;
		string code_block = "";

		if (context.codeBlock() is not null)
			code_block = Visit(context?.codeBlock());
		if (context.statement() is not null)
			code_block = Visit(context?.statement());

		--contextLevel;

		string conditionalEl = "";

		if (context.conditionalElif() is not null)
			conditionalEl = Visit(context.conditionalElif());
		
		if (context.conditionalElse() is not null)
			conditionalEl = Visit(context.conditionalElse());

		return $"{GetIndent(contextLevel)}if {expression}:\r\n{code_block} \r\n{conditionalEl}";
	}

	public override string VisitConditionalElif([Antlr4.Runtime.Misc.NotNull] ConditionalElifContext context)
	{
		string expression = Visit(context.expression());
		++contextLevel;
		string code_block = "";

		if (context.codeBlock() is not null)
			code_block = Visit(context?.codeBlock());
		if (context.statement() is not null)
			code_block = Visit(context?.statement());

		--contextLevel;

		string conditionalEl = "";

		if (context.conditionalElif() is not null)
			conditionalEl = Visit(context.conditionalElif());

		if (context.conditionalElse() is not null)
			conditionalEl = Visit(context.conditionalElse());

		return $"{GetIndent(contextLevel)}elif {expression}:\r\n{code_block} \r\n{conditionalEl}";
	}

	public override string VisitConditionalElse([Antlr4.Runtime.Misc.NotNull] ConditionalElseContext context)
	{
		++contextLevel;
		string code_block = "";

		if (context.codeBlock() is not null)
			code_block = Visit(context?.codeBlock());
		if (context.statement() is not null)
			code_block = Visit(context?.statement());
		--contextLevel;

		return $"{GetIndent(contextLevel)}else:\r\n{code_block} \r\n";
	}

	public override string VisitCodeBlock(CodeBlockContext context)
	{
		List<string> python = new();

		foreach (var i in context.children)
			python.Add(Visit(i));

		python.RemoveAll(s => string.IsNullOrEmpty(s));

		return string.Join("\r\n", python);
	}

	public override string VisitCycle([Antlr4.Runtime.Misc.NotNull] CycleContext context)
	{
		return Visit(context.children[0]);
	}

	public override string VisitWhileCycle([Antlr4.Runtime.Misc.NotNull] WhileCycleContext context)
	{
		string expression = Visit(context.expression());

		++contextLevel;
		string code_block = "";

		if (context.codeBlock() is not null)
			code_block = Visit(context?.codeBlock());
		if (context.statement() is not null)
			code_block = Visit(context?.statement());

		--contextLevel;

		return $"{GetIndent(contextLevel)}while {expression}:\r\n{code_block}";
	}

	public override string VisitMethod([Antlr4.Runtime.Misc.NotNull] MethodContext context)
	{
		string type = context.TYPE().GetText();
		string var_name = context.VAR_NAME().GetText();
		string args = Visit(context.args());

		++contextLevel;
		string code_block = Visit(context?.codeBlock());

		--contextLevel;

		return $"{GetIndent(contextLevel)}def {var_name}({args}):\r\n{code_block}";
	}

	public override string VisitArgs([Antlr4.Runtime.Misc.NotNull] ArgsContext context)
	{
		List<string> python = new();

		foreach (var i in context.children)
		{
			if (i is DeclarationContext)
				python.Add(Visit(i));
		}

		python.RemoveAll(s => string.IsNullOrEmpty(s));

		return string.Join(", ", python);
	}

	public override string VisitReturnExpr([Antlr4.Runtime.Misc.NotNull] ReturnExprContext context)
	{
		string expression = Visit(context.expression());

		return $"{GetIndent(contextLevel)}return {expression}";
	}

	public override string VisitFuncCall([Antlr4.Runtime.Misc.NotNull] FuncCallContext context)
	{
		string var_name = context.VAR_NAME().GetText();
		string args = Visit(context.argsCall());

		return $"{var_name}({args})";
	}

	public override string VisitArgsCall([Antlr4.Runtime.Misc.NotNull] ArgsCallContext context)
	{
		List<string> python = new();

		foreach (var i in context.children)
		{
			if (i is PrimaryLiteralContext)
				python.Add(Visit(i));
		}

		python.RemoveAll(s => string.IsNullOrEmpty(s));

		return string.Join(", ", python);
	}
}