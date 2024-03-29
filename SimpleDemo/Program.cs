﻿using System;
using F;
namespace SimpleDemo {
    class Program {
		static void Main() {
			var ident = FA.Parse("[A-Z_a-z][0-9A-Z_a-z]*", 0, false);
			var num = FA.Parse("0|-?[1-9][0-9]*", 1, false);
			var ws = FA.Parse("[ ]+", 2, false);

			// our expression
			var exp = @"[A-Z_a-z][A-Z_a-z0-9]*|0|\-?[1-9][0-9]*";

			var sa = "the quick brown fox jumped over the -10 $#(%*& lazy dog".Split(' ');
			var proto = new CompiledProto();
			foreach(var s in sa)
			{
				Console.WriteLine("{0}:{1}", s, proto.Match(s));
			}
			Console.WriteLine();
			// search through a string
			foreach (var match in FA.Parse(exp).Search("the quick brown fox jumped over the -10 lazy dog"))
			{
				Console.WriteLine("{0} at {1}", match.Value, match.Position);
			}
	
			// parse it into AST
			var ast = RegexExpression.Parse(exp);
			// visit the AST
			ast.Visit((parent, expr) => { Console.WriteLine(expr.GetType().Name +" "+ expr); return true; });
			// turn it into a state machine
			var nfa = ast.ToFA(0,false);
			// set the ids, which mark the states
			nfa.SetIds();
			// clone the path to state 12
			var cloned = nfa.ClonePathTo(nfa.FindFirst((fa) => { return fa.Id == 12; }));
			// spit the cloned path out 
			cloned.RenderToFile(@"..\..\..\cloned_nfa.jpg");
			Console.WriteLine("-10 is match: {0}", nfa.Match("-10")>-1);
			var opts = new FADotGraphOptions();
			// show accept symbols
			opts.HideAcceptSymbolIds = false;
			// map symbol ids to names
			opts.AcceptSymbolNames = new string[] { "accept" };
			// uncomment to hide expanded epsilons
			//nfa.Compact();
			// compute the NFA table
			var array = nfa.ToArray();
			Console.WriteLine("NFA table length is {0} entries.",array.Length);
			// rebuild the NFA from the table
			nfa = FA.FromArray(array);
			// make a jpg
			nfa.RenderToFile(@"..\..\..\expression_nfa.jpg",opts);
			// make a dot file
			nfa.RenderToFile(@"..\..\..\expression_nfa.dot", opts);
			// make a DFA
			var dfa = nfa.ToDfa();
			// optimize the DFA
			var mdfa = dfa.ToMinimized();
			// make a DFA table
			array = mdfa.ToArray();
			Console.WriteLine("Min DFA table length is {0} entries.", array.Length);
			// search through a string
			foreach (var match in FA.Search(array, "the quick brown fox jumped over the -10 lazy dog"))
			{
				Console.WriteLine("{0} at {1}", match.Value, match.Position);
			}
			// make a jpg
			mdfa.RenderToFile(@"..\..\..\expression_dfa_min.jpg",opts);
			// make a dot file
			mdfa.RenderToFile(@"..\..\..\expression_dfa_min.dot", opts);
			opts.AcceptSymbolNames = new string[] { "ident", "num", "ws" };
			
			var lexer = FA.ToLexer(new FA[] { ident, num, ws },false,false);
			Console.WriteLine("NFA Lexer state count is {0}", lexer.FillClosure().Count);
			lexer.RenderToFile(@"..\..\..\lexer_nfa.jpg", opts);
			foreach (var match in lexer.Search("the quick brown fox jumped over the -10 lazy dog"))
			{
				Console.WriteLine("{0}:{1} at {2}", match.SymbolId, match.Value, match.Position);
			}
			lexer = FA.ToLexer(new FA[] { ident, num, ws }, true);
			array = lexer.ToArray();
			Console.WriteLine("DFA Lexer table length is {0} entries.", array.Length);
			lexer.RenderToFile(@"..\..\..\lexer_dfa.jpg", opts);
			foreach (var match in FA.Search(array, "the quick brown fox jumped over the -10 lazy dog"))
			{
				Console.WriteLine("{0}:{1} at {2}", match.SymbolId, match.Value, match.Position);
			}

		}
	}
}
