﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using F;
using System.Text.RegularExpressions;
namespace CompileDemo
{
	internal class Program
	{
		static string[] _exprs =
		{
			"[[:IsLetter:]_][[:IsLetterOrDigit:]_]*",
			"0|-?[1-9][0-9]*(\\.[0-9]+([Ee]-?[1-9][0-9]*)?)?",
			"[ \t\r\n]+"
		};
		const int _times = 3;
		const int _iterations = 3000;
		const int _divisor = _iterations / 100;
		static string _search;
		const char _block = '■';
		const string _back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
		static void _WriteProgressBar(int percent, bool update = false)
		{
			if (update)
				Console.Write(_back);
			Console.Write("[");
			var p = (int)((percent / 10f) + .5f);
			for (var i = 0; i < 10; ++i)
			{
				if (i >= p)
					Console.Write(' ');
				else
					Console.Write(_block);
			}
			Console.Write("] {0,3:##0}%", percent);
		}
		static void _RunMS()
		{
			var expr = string.Join("|", _exprs);
			Regex rx = new Regex(expr);
			var e = rx.Matches(_search).GetEnumerator();
			while (e.MoveNext()) ;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Console.Write("Microsoft Regex \"Lexer\": ");
			_WriteProgressBar(0);
			for (int i = 0; i < _iterations; ++i)
			{
				e = rx.Matches(_search).GetEnumerator();
				while (e.MoveNext()) ;
				_WriteProgressBar(i / _divisor, true);
			}
			_WriteProgressBar(100, true);
			sw.Stop();
			Console.WriteLine(" Done in {0}ms", sw.ElapsedMilliseconds);
		}
		static void _RunMSCompiled()
		{
			var expr = string.Join("|", _exprs);
			Regex rx = new Regex(expr,RegexOptions.Compiled);
			var e = rx.Matches(_search).GetEnumerator();
			while (e.MoveNext()) ;
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Console.Write("Microsoft Regex Compiled \"Lexer\": ");
			_WriteProgressBar(0);
			for (int i = 0; i < _iterations; ++i)
			{
				e = rx.Matches(_search).GetEnumerator();
				while (e.MoveNext()) ;
				_WriteProgressBar(i / _divisor, true);
			}
			_WriteProgressBar(100, true);
			sw.Stop();
			Console.WriteLine(" Done in {0}ms", sw.ElapsedMilliseconds);
		}
		static void _RunExpandedNfa()
		{
			var exprs = new FA[_exprs.Length];
			for (var i = 0;i<exprs.Length; ++i)
			{
				exprs[i]= FA.Parse(_exprs[i], i, false);
			}
			var lexer = FA.ToLexer(exprs, false);
			using(IEnumerator<FAMatch> e = lexer.Search(_search).GetEnumerator())
			{
				e.MoveNext();
			}
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Console.Write("Expanded NFA Lexer: ");
			_WriteProgressBar(0);
			for(int i = 0;i<_iterations;++i)
			{
				using (IEnumerator<FAMatch> e = lexer.Search(_search).GetEnumerator())
				{
					e.MoveNext();
				}
				_WriteProgressBar(i/_divisor,true);
			}
			_WriteProgressBar(100, true);
			sw.Stop();
			Console.WriteLine(" Done in {0}ms", sw.ElapsedMilliseconds);
		}
		static void _RunCompactNfa()
		{
			var exprs = new FA[_exprs.Length];
			for (var i = 0; i < exprs.Length; ++i)
			{
				exprs[i] = FA.Parse(_exprs[i], i, false);
			}
			var lexer = FA.ToLexer(exprs, false);
			lexer.Compact();
			using (IEnumerator<FAMatch> e = lexer.Search(_search).GetEnumerator())
			{
				e.MoveNext();
			}
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Console.Write("Compacted NFA Lexer: ");
			_WriteProgressBar(0);
			for (int i = 0; i < _iterations; ++i)
			{
				using (IEnumerator<FAMatch> e = lexer.Search(_search).GetEnumerator())
				{
					e.MoveNext();
				}
				_WriteProgressBar(i / _divisor, true);
			}
			_WriteProgressBar(100, true);
			sw.Stop();
			Console.WriteLine(" Done in {0}ms", sw.ElapsedMilliseconds);
		}
		static void _RunUnoptimizedDfa()
		{
			var exprs = new FA[_exprs.Length];
			for (var i = 0; i < exprs.Length; ++i)
			{
				exprs[i] = FA.Parse(_exprs[i], i, false);
			}
			var lexer = FA.ToLexer(exprs, false);
			lexer.ToDfa();
			using (IEnumerator<FAMatch> e = lexer.Search(_search).GetEnumerator())
			{
				e.MoveNext();
			}
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Console.Write("Unoptimized DFA Lexer: ");
			_WriteProgressBar(0);
			for (int i = 0; i < _iterations; ++i)
			{
				using (IEnumerator<FAMatch> e = lexer.Search(_search).GetEnumerator())
				{
					e.MoveNext();
				}
				_WriteProgressBar(i / _divisor, true);
			}
			_WriteProgressBar(100, true);
			sw.Stop();
			Console.WriteLine(" Done in {0}ms", sw.ElapsedMilliseconds);
		}
		static void _RunOptimizedDfa()
		{
			var exprs = new FA[_exprs.Length];
			for (var i = 0; i < exprs.Length; ++i)
			{
				exprs[i] = FA.Parse(_exprs[i], i);
			}
			var lexer = FA.ToLexer(exprs, true);
			using (IEnumerator<FAMatch> e = lexer.Search(_search).GetEnumerator())
			{
				e.MoveNext();
			}
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Console.Write("Optimized DFA Lexer: ");
			_WriteProgressBar(0);
			for (int i = 0; i < _iterations; ++i)
			{
				using (IEnumerator<FAMatch> e = lexer.Search(_search).GetEnumerator())
				{
					e.MoveNext();
				}
				_WriteProgressBar(i / _divisor, true);
			}
			_WriteProgressBar(100, true);
			sw.Stop();
			Console.WriteLine(" Done in {0}ms", sw.ElapsedMilliseconds);
		}
		static void _RunTableDfa()
		{
			var exprs = new FA[_exprs.Length];
			for (var i = 0; i < exprs.Length; ++i)
			{
				exprs[i] = FA.Parse(_exprs[i], i);
			}
			var lexer = FA.ToLexer(exprs, true);
			var table = lexer.ToArray();
			using (IEnumerator<FAMatch> e = FA.Search(table,_search).GetEnumerator())
			{
				e.MoveNext();
			}
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Console.Write("Table based DFA Lexer: ");
			_WriteProgressBar(0);
			for (int i = 0; i < _iterations; ++i)
			{
				using (IEnumerator<FAMatch> e = FA.Search(table,_search).GetEnumerator())
				{
					e.MoveNext();
				}
				_WriteProgressBar(i / _divisor, true);
			}
			_WriteProgressBar(100, true);
			sw.Stop();
			Console.WriteLine(" Done in {0}ms", sw.ElapsedMilliseconds);
		}
		static void _RunCompiledDfa()
		{
			var exprs = new FA[_exprs.Length];
			for (var i = 0; i < exprs.Length; ++i)
			{
				exprs[i] = FA.Parse(_exprs[i], i);
			}
			var lexer = FA.ToLexer(exprs, true);
			var runner = lexer.Compile();
			using (IEnumerator<FAMatch> e = runner.Search(_search).GetEnumerator())
			{
				e.MoveNext();
			}
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Console.Write("Compiled DFA Lexer: ");
			_WriteProgressBar(0);
			for (int i = 0; i < _iterations; ++i)
			{
				using (IEnumerator<FAMatch> e = runner.Search(_search).GetEnumerator())
				{
					e.MoveNext();
				}
				_WriteProgressBar(i / _divisor, true);
			}
			_WriteProgressBar(100, true);
			sw.Stop();
			Console.WriteLine(" Done in {0}ms", sw.ElapsedMilliseconds);
		}
		static void _Bench()
		{
			using(var sw = new StreamReader(@"..\..\Program.cs"))
			{
				_search = sw.ReadToEnd();
			}
#if DEBUG
			Console.WriteLine("Running in Debug mode which is significantly slower.");
			Console.WriteLine();
#endif
			for (int i = 0; i < _times; ++i)
			{
				_RunMS();
				_RunMSCompiled();
				_RunExpandedNfa();
				_RunCompactNfa();
				_RunUnoptimizedDfa();
				_RunOptimizedDfa();
				_RunTableDfa();
				_RunCompiledDfa();
				Console.WriteLine();
			}
		}
		static void _TestMatch()
		{
			var exprs = new FA[_exprs.Length];
			for (var i = 0; i < exprs.Length; ++i)
			{
				exprs[i] = FA.Parse(_exprs[i], i, false);
			}
			var lexer = FA.ToLexer(exprs, true);
			var runner = lexer.Compile();
			foreach (var s in "The quick brown fox jumped over -10 the #*(@$& lazy dog".Split(' '))
			{
				Console.WriteLine("Match {0}: {1}",s,runner.Match(s));
			}
		}
		static void Main()
		{
			//_TestMatch();
			_Bench();
		}
	}
}
