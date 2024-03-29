﻿// Portions of this code adopted from Fare, itself adopted from brics
// original copyright notice included.
// This is the only file this applies to.

/*
 * 
* The MIT License (MIT)
* 
* Copyright (c) 2013 Nikos Baxevanis
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE
* 
 * dk.brics.automaton
 * 
 * Copyright (c) 2001-2011 Anders Moeller
 * All rights reserved.
 * http://github.com/moodmosaic/Fare/
 * Original Java code:
 * http://www.brics.dk/automaton/
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using LC;
namespace F
{
#if FALIB
	public
#endif
	abstract partial class FARunner
	{
		internal class Enumerator : IEnumerator<FAMatch>
		{
			WeakReference<FARunner> _parent;
			LexContext _context;
			FAMatch _current;
			internal Enumerator(FARunner parent,LexContext context)
			{
				_parent = new WeakReference<FARunner>(parent);
				_context = context;
			}
			public FAMatch Current { get { return _current; } }
			object System.Collections.IEnumerator.Current { get { return Current; } } 
			
			public bool MoveNext()
			{
				_context.EnsureStarted();
				if(_context.Current == LexContext.EndOfInput)
				{
					return false;
				}
				FARunner parent;
				if(!_parent.TryGetTarget(out parent))
				{
					throw new InvalidOperationException("The parent was destroyed");
				}
				_context.ClearCapture();
				_current = parent.SearchImpl(_context);
				return true;
			}
			void System.Collections.IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}
			void System.IDisposable.Dispose()
			{
				
			}
		}
		internal class Enumerable : IEnumerable<FAMatch>
		{
			WeakReference<FARunner> _parent;
			LexContext _context;
			internal Enumerable(FARunner parent, LexContext context)
			{
				_parent = new WeakReference<FARunner>(parent);
				_context = context;
			}
			public IEnumerator<FAMatch> GetEnumerator()
			{
				FARunner parent;
				if (!_parent.TryGetTarget(out parent))
				{
					throw new InvalidOperationException("The parent was destroyed");
				}
				return new Enumerator(parent,_context);
			}
			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
		}
		protected abstract int MatchImpl(LexContext lc);
		protected abstract FAMatch SearchImpl(LexContext lc);
		/// <summary>
		/// Searches through text for the next occurance of a pattern matchable by this machine
		/// </summary>
		/// <param name="lc">The <see cref="LexContext"/> with the text to search</param>
		/// <returns>A series of <see cref="FAMatch"/> instances with the matches</returns>
		public IEnumerable<FAMatch> Search(LexContext lc)
		{
			return new Enumerable(this, lc);
		}
		/// <summary>
		/// Indicates whether this machine will match the indicated text
		/// </summary>
		/// <param name="lc">A <see cref="LexContext"/> containing the text</param>
		/// <returns>The accept symbol if matched, otherwise -1.</returns>
		public int Match(LexContext lc)
		{
			lc.EnsureStarted();
			return MatchImpl(lc);
		}
		/// <summary>
		/// Searches through text for the next occurance of a pattern matchable by this machine
		/// </summary>
		/// <param name="@string">The <see cref="IEnumerable{char}"/> with the text to search</param>
		/// <returns>A series of <see cref="FAMatch"/> instances with the matches</returns>
		public IEnumerable<FAMatch> Search(IEnumerable<char> @string)
		{
			return Search(LexContext.Create(@string));
		}
		/// <summary>
		/// Searches through text for the next occurance of a pattern matchable by this machine
		/// </summary>
		/// <param name="reader">The <see cref="TextReader"/> with the text to search</param>
		/// <returns>A series of <see cref="FAMatch"/> instances with the matches</returns>
		public IEnumerable<FAMatch> Search(TextReader reader)
		{
			return Search(LexContext.CreateFrom(reader));
		}
		/// <summary>
		/// Indicates whether this machine will match the indicated text
		/// </summary>
		/// <param name="text">The text</param>
		/// <returns>True if the passed in text was a match, otherwise false.</returns>
		public int Match(IEnumerable<char> text)
		{
			return Match(LexContext.Create(text));
		}

	}
	/// <summary>
	/// Represents a match from <code>FA.Search()</code>
	/// </summary>
#if FALIB
	public
#endif
	partial struct FAMatch
	{
		/// <summary>
		/// The matched symbol - this is the accept id
		/// </summary>
		public int SymbolId { get; private set; }
		/// <summary>
		/// The matched value
		/// </summary>
		public string Value { get; private set; }
		/// <summary>
		/// The position of the match within the codepoint series - this may not be the same as the character position due to surrogates
		/// </summary>
		public long Position { get; private set; }
		/// <summary>
		/// The one based line number
		/// </summary>
		public int Line { get; private set; }
		/// <summary>
		/// The one based column
		/// </summary>
		public int Column { get; private set; }
		/// <summary>
		/// Constructs a new instance
		/// </summary>
		/// <param name="symbolId">The symbol id</param>
		/// <param name="value">The matched value</param>
		/// <param name="position">The absolute codepoint position</param>
		/// <param name="line">The line</param>
		/// <param name="column">The column</param>
		
		public static FAMatch Create(int symbolId, string value, long position, int line, int column)
		{
			FAMatch result=default(FAMatch);
			result.SymbolId = symbolId;
			result.Value = value;
			result.Position = position;
			result.Line = line;
			result.Column = column;
			return result;
		}
	}
	/// <summary>
	/// Represents a transition within a state
	/// </summary>
#if FALIB
	public
#endif
	partial struct FATransition
	{
		/// <summary>
		/// The minimum codepoint. For epsilons this is -1.
		/// </summary>
		public int Min;
		/// <summary>
		/// The maximum codepoint. For epsilons this is -1.
		/// </summary>
		public int Max;
		/// <summary>
		/// The state to transition to
		/// </summary>
		public FA To;
		/// <summary>
		/// Constructs a new instance
		/// </summary>
		/// <param name="min">The minumum codepoint</param>
		/// <param name="max">The maximum codepoint</param>
		/// <param name="to">The state to transition to</param>
		public FATransition(int min, int max, FA to)
		{
			Min = min;
			Max = max;
			To = to;
		}

	}
	/// <summary>
	/// The filter predicate delegate for <see cref="FA.FindFirst(FAFindFilter)" and <see cref="FA.FillFind(FAFindFilter, IList{FA})"/>/>
	/// </summary>
	/// <param name="state">The state to check</param>
	/// <returns>True if matched, otherwise false</returns>
#if FALIB
	public
#endif
	delegate bool FAFindFilter(FA state);
	/// <summary>
	/// Represents a single state in a state machine
	/// </summary>
#if FALIB
	public
#endif
	partial class FA : FARunner
	{
		/// <summary>
		/// This state has collapsed epsilons
		/// </summary>
		public bool IsCompact { get; private set; } = true;
		/// <summary>
		/// This state has no overlapping transitions and no epsilon transitions
		/// </summary>
		public bool IsDeterministic { get; private set; } = true;
		/// <summary>
		/// The symbol id if this state is accepting, otherwise -1
		/// </summary>
		public int AcceptSymbol { get; set; } = -1;
		/// <summary>
		/// A value used for state minimization
		/// </summary>
		private int Tag { get; set; } = 0;
		/// <summary>
		/// The list of states this state was constructed from, if applicable, otherwise null
		/// </summary>
		public FA[] FromStates { get; private set; } = null;
		/// <summary>
		/// The id of the state, used for debugging
		/// </summary>
		public int Id { get; set; } = -1;
		/// <summary>
		/// A filter that returns any accepting state
		/// </summary>
		public static readonly FAFindFilter AcceptingFilter = new FAFindFilter((FA state)=>{ return state.IsAccepting; });
		/// <summary>
		/// A filter that returns any final states
		/// </summary>
		public static readonly FAFindFilter FinalFilter = new FAFindFilter((FA state) => { return state.IsFinal; });
		/// <summary>
		/// A filter that returns any neutral states
		/// </summary>
		public static readonly FAFindFilter NeutralFilter = new FAFindFilter((FA state) => { return state.IsNeutral; });
		/// <summary>
		/// The list of transitions from this state
		/// </summary>
		public readonly List<FATransition> Transitions = new List<FATransition>();
		/// <summary>
		/// Constructs a new instance
		/// </summary>
		/// <param name="acceptSymbol">The symbol to accept, or -1</param>
		public FA(int acceptSymbol)
		{
			AcceptSymbol = acceptSymbol;
		}
		/// <summary>
		/// Constructs a new instance
		/// </summary>
		public FA() { }
		/// <summary>
		/// Indicates if the state accepts. False if it doesn't, otherwise true
		/// </summary>
		public bool IsAccepting {
			get {
				return AcceptSymbol > -1;
			}
		}
		/// <summary>
		/// Indicates if the state has no transitions
		/// </summary>
		public bool IsFinal { get { return 0 == Transitions.Count; } }
		/// <summary>
		/// Indicates if the state does not change the accepted input
		/// </summary>
		public bool IsNeutral {
			get {
				if(!IsAccepting && 1==Transitions.Count)
				{
					var fat = Transitions[0];
					if(fat.Min==-1 && fat.Max==-1)
					{
						return true;
					}
				}
				return false;
			}
		}
		/// <summary>
		/// Adds an epsilon transition to the machine
		/// </summary>
		/// <param name="to">The destination state</param>
		/// <param name="compact">True to copy the epsilon data directly to this state, collapsing it, otherwise false to keep the epsilon in expanded form</param>
		public void AddEpsilon(FA to, bool compact = true)
		{
			IsDeterministic = false;
			if (compact)
			{
				if (to.IsAccepting && !IsAccepting)
				{
					AcceptSymbol = to.AcceptSymbol;
				}
				for (int ic = to.Transitions.Count, i = 0; i < ic; ++i)
				{
					Transitions.Add(to.Transitions[i]);
				}
				return;
			}
			FATransition fat;
			fat.Min = -1;
			fat.Max = -1;
			fat.To = to;
			Transitions.Add(fat);
			IsCompact = false;
		}
		/// <summary>
		/// Set the ids for each state in this machine
		/// </summary>
		public void SetIds()
		{
			var cls = new List<FA>();
			FillClosure(cls);
			var closure = cls.ToArray();
			for (int i = 0; i < closure.Length;++i)
			{
				closure[i].Id = i;
			}
		}
		/// <summary>
		/// Converts the state to a string.
		/// </summary>
		/// <remarks>If the id is set, this will report it.</remarks>
		/// <returns></returns>
		public override string ToString()
		{
			if (Id < 0)
			{
				return base.ToString();
			} else
			{
				return "q" + Id.ToString();
			}
		}
		void _Closure(IList<FA> result, ISet<FA> seen)
		{
			if(!seen.Add(this))
			{
				return;
			}
			result.Add(this);
			for (int ic = Transitions.Count, i = 0; i < ic; ++i)
			{
				var t = Transitions[i];
				t.To._Closure(result,seen);
			}
		}
		/// <summary>
		/// Computes the closure of this state into a list.
		/// </summary>
		/// <remarks>The closure is the list of states reachable from this state including itself. It essentially lists the states that make up the machine. This state is always the first state in the list.</remarks>
		/// <param name="result">The list to fill</param>
		/// <returns>A list filled with the closure. If <paramref name="result"/> is specified, that instance will be filled and returned. Otherwise a new list is filled and returned.</returns>
		public IList<FA> FillClosure(IList<FA> result = null)
		{
			if (null == result)
				result = new List<FA>();
			var set = new HashSet<FA>();
			_Closure(result, set);
			return result;
		}
		void _Find(FAFindFilter filter, IList<FA> result, ISet<FA> seen)
		{
			if (!seen.Add(this))
			{
				return;
			}
			if (filter(this))
			{
				result.Add(this);
			}
			for (int ic = Transitions.Count, i = 0; i < ic; ++i)
			{
				var t = Transitions[i];
				t.To._Find(filter,result, seen);
			}
		}
		/// <summary>
		/// Finds states within the closure that match the filter criteria
		/// </summary>
		/// <param name="filter">The filter predicate to use.</param>
		/// <param name="result">The result to fill</param>
		/// <returns>A list filled with the result of the find. If <paramref name="result"/> is specified, that instance will be filled and returned. Otherwise a new list is filled and returned.</returns>
		public IList<FA> FillFind(FAFindFilter filter, IList<FA> result = null)
		{
			if (null == result)
				result = new List<FA>();
			var set = new HashSet<FA>();
			_Find(filter, result, set);
			return result;
		}
		FA _FindFirst(FAFindFilter filter, ISet<FA> seen)
		{
			if (!seen.Add(this))
			{
				return null;
			}
			if (filter(this))
			{
				return this;
			}
			for (int ic = Transitions.Count, i = 0; i < ic; ++i)
			{
				var t = Transitions[i];
				var fa = t.To._FindFirst(filter, seen);
				if(null!=fa)
				{
					return fa;
				}
			}
			return null;
		}
		/// <summary>
		/// Finds the first state within the closure that matches the filter critera
		/// </summary>
		/// <param name="filter">The filter predicate to use</param>
		/// <returns>The first state that matches the filter criteria, or null if not found.</returns>
		public FA FindFirst(FAFindFilter filter)
		{
			var set = new HashSet<FA>();
			return _FindFirst(filter, set);
		}
		/// <summary>
		/// Computes the epsilon closure of this machine.
		/// </summary>
		/// <remarks>The epsilon closure is the list of all states reachable from this state on no input.</remarks>
		/// <param name="result">The result to fill or null</param>
		/// <returns>A list filled with the epsilon closure. If <paramref name="result"/> is specified, that instance will be filled and returned. Otherwise a new list is filled and returned.</returns>
		public IList<FA> FillEpsilonClosure(IList<FA> result = null)
		{
			if (null == result)
				result = new List<FA>();
			if (result.Contains(this))
			{
				return result;
			}
						
			if (result.Contains(this))
				return result;
			result.Add(this);
			for (int ic = Transitions.Count, i = 0; i < ic; ++i)
			{
				var t = Transitions[i];
				if (t.Min == -1 && t.Max == -1)
				{
					t.To.FillEpsilonClosure(result);
				}
			}
			return result;
		}
		/// <summary>
		/// Computes the total epsilon closure of a list of states
		/// </summary>
		/// <remarks>The epsilon closure is the list of all states reachable from these states on no input.</remarks>
		/// <param name="states">The states to compute on</param>
		/// <param name="result">The result to fill, or null if a new list is to be returned. This parameter is required in order to disambiguate with the instance method of the same name.</param>
		/// <returns></returns>
		public static IList<FA> FillEpsilonClosure(IEnumerable<FA> states, IList<FA> result)
		{
			if (null == result)
				result = new List<FA>();
			var set = new HashSet<FA>();
			foreach(var fa in states)
			{
				var epsc = fa.FillEpsilonClosure();
				foreach (var efa in epsc)
				{
					if(set.Add(efa))
					{
						result.Add(efa);
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Creates a new set of states that represent the path to a given state, excluding other states.
		/// </summary>
		/// <param name="to">The state to traverse to</param>
		/// <returns>A new set of states from cloned from this machine which lead to <paramref name="to"/></returns>
		public FA ClonePathTo(FA to)
   		{
   			var closure = FillClosure();
   			var nclosure = new FA[closure.Count];
   			for (var i = 0; i < nclosure.Length; i++)
   			{
   				nclosure[i] = new FA(closure[i].AcceptSymbol);
   				nclosure[i].Tag = closure[i].Tag;
				nclosure[i].Id = closure[i].Id;
				nclosure[i].FromStates = closure[i].FromStates;
   			}
   			for (var i = 0; i < nclosure.Length; i++)
   			{
   				var t = nclosure[i].Transitions;
   				foreach (var trns in closure[i].Transitions)
   				{
					if (trns.To.FindFirst((fa) => { return (fa == to); })!=null)
   					{
   						var id = closure.IndexOf(trns.To);
   						t.Add(new FATransition(trns.Min,trns.Max, nclosure[id]));
   					}
   				}
   			}
   			return nclosure[0];
   		}
		/// <summary>
		/// Computes state indices that represent the path to a given state, excluding other states.
		/// </summary>
		/// <param name="to">The state to traverse to</param>
		/// <returns>An array of indices this machine which lead to <paramref name="to"/></returns>
		public int[] PathToIndices(FA to)
		{
			var closure = FillClosure();
			var result = new List<int>(closure.Count);
			for(int i = 0;i<closure.Count;++i)
			{
				var fa = closure[i];
				if(fa.FindFirst((ffa) => { return (ffa == to); }) != null)
				{
					result.Add(i);
				}
			}
			return result.ToArray();
		}
		/// <summary>
		/// Finds the accepting states of this machine.
		/// </summary>
		/// <param name="result">The result to fill or null</param>
		/// <returns>A list filled with the accepting states. If <paramref name="result"/> is specified, that instance will be filled and returned. Otherwise a new list is filled and returned.</returns>
		public IList<FA> FillAcceptingStates(IList<FA> result = null)
		{
			return FillFind(AcceptingFilter, result);
		}
		/// <summary>
		/// Finds the accepting states of the closure.
		/// </summary>
		/// <param name="closure">The closure to search</param>
		/// <param name="result">The result to fill or null</param>
		/// <returns>A list filled with the accepting states. If <paramref name="result"/> is specified, that instance will be filled and returned. Otherwise a new list is filled and returned.</returns>
		public static IList<FA> FillAcceptingStates(IList<FA> closure, IList<FA> result = null)
		{
			if (null == result)
				result = new List<FA>();
			for (int ic = closure.Count, i = 0; i < ic; ++i)
			{
				var fa = closure[i];
				if (fa.IsAccepting)
					result.Add(fa);
			}
			return result;
		}
		/// <summary>
		/// Computes a dictionary keyed by states, whose values are the ranges that lead to that state packed as an integer array.
		/// </summary>
		/// <param name="includeEpsilons">Indicates that epsilon transitions should be included in the result</param>
		/// <param name="result">The dictionary to fill, or null for a new dictionary</param>
		/// <returns>A dictionary of packed sorted ranges grouped by destination state. If <paramref name="result"/> was specified, that will be returned.</returns>
		public IDictionary<FA, int[]> FillInputTransitionRangesGroupedByState(bool includeEpsilons = false, IDictionary<FA, int[]> result = null)
		{
			var working = new Dictionary<FA, List<KeyValuePair<int, int>>>();
			foreach (var trns in Transitions)
			{
				if (!includeEpsilons && (trns.Min == -1 && trns.Max == -1))
				{
					continue;
				}
				List<KeyValuePair<int, int>> l;
				if (!working.TryGetValue(trns.To, out l))
				{
					l = new List<KeyValuePair<int, int>>();
					working.Add(trns.To, l);
				}
				l.Add(new KeyValuePair<int, int>(trns.Min, trns.Max));
			}
			
			if (null == result)
				result = new Dictionary<FA, int[]>();
			foreach (var item in working)
			{
				item.Value.Sort((x, y) => { var c = x.Key.CompareTo(y.Key); if (0 != c) return c; return x.Value.CompareTo(y.Value); });
				_NormalizeSortedRangeList(item.Value);
				result.Add(item.Key, FromPairs(item.Value));
			}
			return result;
		}
		static void _NormalizeSortedRangeList(IList<KeyValuePair<int, int>> pairs)
		{

			var or = default(KeyValuePair<int, int>);
			for (int i = 1; i < pairs.Count; ++i)
			{
				if (pairs[i - 1].Value + 1 >= pairs[i].Key)
				{
					var nr = new KeyValuePair<int, int>(pairs[i - 1].Key, pairs[i].Value);
					pairs[i - 1] = or = nr;
					pairs.RemoveAt(i);
					--i; // compensated for by ++i in for loop
				}
			}
		}
		/// <summary>
		/// Creates a deep copy of the current machine
		/// </summary>
		/// <returns>A new machine that is a duplicate of this machine</returns>
		public FA Clone() { return Clone(FillClosure()); }
		/// <summary>
		/// Creates a deep copy of the closure
		/// </summary>
		/// <param name="closure">The closure to copy</param>
		/// <returns>A new machine that has a deep copy of the given closure</returns>
		public static FA Clone(IList<FA> closure)
		{
			var nclosure = new FA[closure.Count];
			for (var i = 0; i < nclosure.Length; i++)
			{
				var fa = closure[i];
				var nfa = new FA();
				nfa.AcceptSymbol = fa.AcceptSymbol;
				nfa.IsDeterministic = fa.IsDeterministic;
				nclosure[i] = nfa;
			}
			for (var i = 0; i < nclosure.Length; i++)
			{
				var fa = closure[i];
				var nfa = nclosure[i];
				for (int jc = fa.Transitions.Count, j = 0; j < jc; ++j)
				{
					var t = fa.Transitions[j];
					nfa.Transitions.Add(new FATransition(t.Min, t.Max, nclosure[closure.IndexOf(t.To)]));
				}
			}
			return nclosure[0];
		}
		/// <summary>
		/// Creates a literal machine given the UTF-32 string
		/// </summary>
		/// <remarks>Use <code>ToUtf32()</code> to compute from characters.</remarks>
		/// <param name="string">The codepoints to create the literal from.</param>
		/// <param name="accept">The accepting id</param>
		/// <param name="compact">True to collapse epsilons, false to generate expanded epsilons</param>
		/// <returns>A new machine representing the literal expression</returns>
		public static FA Literal(IEnumerable<int> @string, int accept = 0, bool compact = true)
		{
			var result = new FA();
			var current = result;
			foreach (var ch in @string)
			{
				current.AcceptSymbol = -1;
				var fa = new FA();
				fa.AcceptSymbol = accept;
				current.Transitions.Add(new FATransition(ch, ch, fa));
				current = fa;
			}
			return result;
		}
		/// <summary>
		/// Creates a charset machine represeting the given the UTF-32 codepoint ranges
		/// </summary>
		/// <param name="ranges">The <see cref="KeyValuePair{Int32, Int32}"/> codepoint ranges to create the set from.</param>
		/// <param name="accept">The accepting id</param>
		/// <param name="compact">True to collapse epsilons, false to generate expanded epsilons</param>
		/// <returns>A new machine representing the set expression</returns>
		public static FA Set(IEnumerable<KeyValuePair<int, int>> ranges, int accept = 0, bool compact = true)
		{
			var result = new FA();
			var final = new FA(accept);
			var pairs = new List<KeyValuePair<int, int>>(ranges);
			pairs.Sort((x, y) => { var c = x.Key.CompareTo(y.Key); if (0 != c) return c; return x.Value.CompareTo(y.Value); });
			foreach (var pair in pairs)
				result.Transitions.Add(new FATransition(pair.Key, pair.Value, final));

			return result;
		}
		/// <summary>
		/// Creates a machine that is a concatenation of the given expressions
		/// </summary>
		/// <param name="exprs">The expressions to concatenate</param>
		/// <param name="accept">The accept id</param>
		/// <param name="compact">True to collapse epsilons, false to generate expanded epsilons</param>
		/// <returns>A new machine representing the concatenated expressions</returns>
		public static FA Concat(IEnumerable<FA> exprs, int accept = 0, bool compact = true)
		{
			FA result = null, left = null, right = null;
			foreach (var val in exprs)
			{
				if (null == val) continue;
				//Debug.Assert(null != val.FirstAcceptingState);
				var nval = val.Clone();
				//Debug.Assert(null != nval.FirstAcceptingState);
				if (null == left)
				{
					if (null == result)
						result = nval;
					left = nval;
					//Debug.Assert(null != left.FirstAcceptingState);
					continue;
				}
				if (null == right)
				{
					right = nval;
				}

				//Debug.Assert(null != left.FirstAcceptingState);
				nval = right.Clone();
				_Concat(left, nval, compact);
				right = null;
				left = nval;

				//Debug.Assert(null != left.FirstAcceptingState);

			}
			if (null != right)
			{
				var acc = right.FillAcceptingStates();
				for (int ic = acc.Count, i = 0; i < ic; ++i)
					acc[i].AcceptSymbol = accept;
			}
			else
			{
				var acc = result.FillAcceptingStates();
				for (int ic = acc.Count, i = 0; i < ic; ++i)
					acc[i].AcceptSymbol = accept;
			}
			return result;
		}
		static void _Concat(FA lhs, FA rhs,bool compact)
		{
			//Debug.Assert(lhs != rhs);
			var acc = lhs.FillAcceptingStates();
			for (int ic = acc.Count, i = 0; i < ic; ++i)
			{
				var f = acc[i];
				f.AcceptSymbol = -1;
				//Debug.Assert(null != rhs.FirstAcceptingState);
				f.AddEpsilon(rhs,compact);
				//Debug.Assert(null!= lhs.FirstAcceptingState);
			}
		}
		/// <summary>
		/// Creates a machine that is a disjunction between several expressions.
		/// </summary>
		/// <param name="exprs">The expressions that represent the possible choices to match</param>
		/// <param name="accept">The accept id</param>
		/// <param name="compact">True to collapse epsilons, false to generate expanded epsilons</param>
		/// <returns>A new machine representing the or expression</returns>
		public static FA Or(IEnumerable<FA> exprs, int accept = 0, bool compact = true)
		{
			var result = new FA();
			var final = new FA(accept);
			foreach (var fa in exprs)
			{
				if (null != fa)
				{
					var nfa = fa.Clone();
					result.AddEpsilon(nfa,compact);
					var acc = nfa.FillAcceptingStates();
					for (int ic = acc.Count, i = 0; i < ic; ++i)
					{
						var nffa = acc[i];
						nffa.AcceptSymbol = -1;
						nffa.AddEpsilon(final,compact);
					}
				}
				else result.AddEpsilon(final,compact);
			}
			return result;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="expr">The expression to make optional</param>
		/// <param name="accept">The accept id</param>
		/// <param name="compact">True to collapse epsilons, false to generate expanded epsilons</param>
		/// <returns>A new machine representing the optional expression</returns>
		public static FA Optional(FA expr, int accept = 0, bool compact = true)
		{
			var result = expr.Clone();
			var acc = result.FillAcceptingStates();
			for (int ic = acc.Count, i = 0; i < ic; ++i)
			{
				var fa = acc[i];
				fa.AcceptSymbol = accept;
				result.AddEpsilon(fa,compact);
			}
			return result;
		}
		/// <summary>
		/// Creates a repetition of the given expression
		/// </summary>
		/// <param name="expr">The expression to repeat</param>
		/// <param name="minOccurs">The minimum number of times <paramref name="expr"/> should occur</param>
		/// <param name="maxOccurs">The maximum number of times <paramref name="expr"/> should occur. Specify 0 or -1 for unbounded.</param>
		/// <param name="accept">The accept id to use</param>
		/// <param name="compact">True to collapse epsilons, false to generate expanded epsilons</param>
		/// <returns>A new machine representing the repeated expression</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="minOccurs"/> or <paramref name="maxOccurs"/> is an invalid value</exception>
		public static FA Repeat(FA expr, int minOccurs = -1, int maxOccurs = -1, int accept = 0,bool compact = true)
		{
			expr = expr.Clone();
			if (minOccurs > 0 && maxOccurs > 0 && minOccurs > maxOccurs)
				throw new ArgumentOutOfRangeException(nameof(maxOccurs));
			FA result;
			switch (minOccurs)
			{
				case -1:
				case 0:
					switch (maxOccurs)
					{
						case -1:
						case 0:
							result = new FA(accept);
							result.AddEpsilon(expr,compact);
							foreach (var afa in expr.FillAcceptingStates())
							{
								afa.AddEpsilon(result, compact);
							}
							return result;
						case 1:
							result = Optional(expr, accept,compact);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
						default:
							var l = new List<FA>();
							expr = Optional(expr,accept,compact);
							l.Add(expr);
							for (int i = 1; i < maxOccurs; ++i)
							{
								l.Add(expr.Clone());
							}
							result = Concat(l, accept,compact);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
					}
				case 1:
					switch (maxOccurs)
					{
						case -1:
						case 0:
							result = Concat(new FA[] { expr, Repeat(expr, 0, 0, accept,compact) }, accept,compact);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
						case 1:
							//Debug.Assert(null != expr.FirstAcceptingState);
							return expr;
						default:
							result = Concat(new FA[] { expr, Repeat(expr, 0, maxOccurs - 1,accept,compact) }, accept,compact);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
					}
				default:
					switch (maxOccurs)
					{
						case -1:
						case 0:
							result = Concat(new FA[] { Repeat(expr, minOccurs, minOccurs, accept,compact), Repeat(expr, 0, 0, accept,compact) }, accept,compact);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
						case 1:
							throw new ArgumentOutOfRangeException(nameof(maxOccurs));
						default:
							if (minOccurs == maxOccurs)
							{
								var l = new List<FA>();
								l.Add(expr);
								//Debug.Assert(null != expr.FirstAcceptingState);
								for (int i = 1; i < minOccurs; ++i)
								{
									var e = expr.Clone();
									//Debug.Assert(null != e.FirstAcceptingState);
									l.Add(e);
								}
								result = Concat(l, accept);
								//Debug.Assert(null != result.FirstAcceptingState);
								return result;
							}
							result = Concat(new FA[] { Repeat(expr, minOccurs, minOccurs, accept,compact), Repeat(Optional(expr,accept,compact), maxOccurs - minOccurs, maxOccurs - minOccurs, accept,compact) }, accept,compact);
							//Debug.Assert(null != result.FirstAcceptingState);
							return result;
					}
			}
		}
		/// <summary>
		/// Makes a machine case insensitive
		/// </summary>
		/// <param name="expr">The expression to make case insensitive</param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException"></exception>
		public static FA CaseInsensitive(FA expr)
		{
			var result = expr.Clone();
			var closure = new List<FA>();
			result.FillClosure(closure);
			for (int ic = closure.Count, i = 0; i < ic; ++i)
			{
				var fa = closure[i];
				var t = new List<FATransition>(fa.Transitions);
				fa.Transitions.Clear();
				foreach (var trns in t)
				{
					var f = char.ConvertFromUtf32(trns.Min);
					var l = char.ConvertFromUtf32(trns.Max);
					if (char.IsLower(f, 0))
					{
						if (!char.IsLower(l, 0))
							throw new NotSupportedException("Attempt to make an invalid range case insensitive");
						fa.Transitions.Add(new FATransition(trns.Min, trns.Max, trns.To));
						f = f.ToUpperInvariant();
						l = l.ToUpperInvariant();
						fa.Transitions.Add(new FATransition(char.ConvertToUtf32(f, 0), char.ConvertToUtf32(l, 0), trns.To));

					}
					else if (char.IsUpper(f, 0))
					{
						if (!char.IsUpper(l, 0))
							throw new NotSupportedException("Attempt to make an invalid range case insensitive");
						fa.Transitions.Add(new FATransition(trns.Min, trns.Max, trns.To));
						f = f.ToLowerInvariant();
						l = l.ToLowerInvariant();
						fa.Transitions.Add(new FATransition(char.ConvertToUtf32(f, 0), char.ConvertToUtf32(l, 0), trns.To));
					}
					else
					{
						fa.Transitions.Add(new FATransition(trns.Min, trns.Max, trns.To));
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Parses an expression from the input
		/// </summary>
		/// <param name="input">The string to parse</param>
		/// <param name="accept">The accept id for the final expression</param>
		/// <param name="compact">True to collapse epsilons, false to expand epsilons</param>
		/// <param name="line">The starting line</param>
		/// <param name="column">The starting column</param>
		/// <param name="position">The starting position</param>
		/// <param name="fileOrUrl">The source file or url</param>
		/// <returns>A new expression parsed from the string</returns>
		public static FA Parse(IEnumerable<char> input, int accept = 0, bool compact = true, int line = 1, int column = 1, long position = 0, string fileOrUrl = null)
		{
			var lc = LexContext.Create(input);
			lc.EnsureStarted();
			lc.SetLocation(line, column, position, fileOrUrl);
			var result = Parse(lc, accept,compact);
			return result;
		}
		/// <summary>
		/// Parses an expression from the input
		/// </summary>
		/// <param name="pc">The <see cref="LexContext"/> to parse</param>
		/// <param name="accept">The accept id for the final expression</param>
		/// <param name="compact">True to collapse epsilons, false to expand epsilons</param>
		/// <returns>A new expression parsed from the <see cref="LexContext"/> instance</returns>
		internal static FA Parse(LexContext pc, int accept = 0, bool compact = true)
		{

			FA result = null, next;
			int ich;
			pc.EnsureStarted();
			while (true)
			{
				switch (pc.Current)
				{
					case -1:
						//result = result.ToMinimized();
						return result;
					case '.':
						var dot = FA.Set(new KeyValuePair<int, int>[] { new KeyValuePair<int, int>(0, 0x10ffff) }, accept,compact);
						if (null == result)
							result = dot;
						else
						{
							result = FA.Concat(new FA[] { result, dot }, accept,compact);
						}
						pc.Advance();
						result = _ParseModifier(result, pc, accept,compact);
						break;
					case '\\':

						pc.Advance();
						pc.Expecting();
						var isNot = false;
						switch (pc.Current)
						{
							case 'P':
								isNot = true;
								goto case 'p';
							case 'p':
								pc.Advance();
								pc.Expecting('{');
								var uc = new StringBuilder();
								int uli = pc.Line;
								int uco = pc.Column;
								long upo = pc.Position;
								while (-1 != pc.Advance() && '}' != pc.Current)
									uc.Append(char.ConvertFromUtf32(pc.Current));
								pc.Expecting('}');
								pc.Advance();
								int uci = 0;
								switch (uc.ToString())
								{
									case "Pe":
										uci = 21;
										break;
									case "Pc":
										uci = 19;
										break;
									case "Cc":
										uci = 14;
										break;
									case "Sc":
										uci = 26;
										break;
									case "Pd":
										uci = 19;
										break;
									case "Nd":
										uci = 8;
										break;
									case "Me":
										uci = 7;
										break;
									case "Pf":
										uci = 23;
										break;
									case "Cf":
										uci = 15;
										break;
									case "Pi":
										uci = 22;
										break;
									case "Nl":
										uci = 9;
										break;
									case "Zl":
										uci = 12;
										break;
									case "Ll":
										uci = 1;
										break;
									case "Sm":
										uci = 25;
										break;
									case "Lm":
										uci = 3;
										break;
									case "Sk":
										uci = 27;
										break;
									case "Mn":
										uci = 5;
										break;
									case "Ps":
										uci = 20;
										break;
									case "Lo":
										uci = 4;
										break;
									case "Cn":
										uci = 29;
										break;
									case "No":
										uci = 10;
										break;
									case "Po":
										uci = 24;
										break;
									case "So":
										uci = 28;
										break;
									case "Zp":
										uci = 13;
										break;
									case "Co":
										uci = 17;
										break;
									case "Zs":
										uci = 11;
										break;
									case "Mc":
										uci = 6;
										break;
									case "Cs":
										uci = 16;
										break;
									case "Lt":
										uci = 2;
										break;
									case "Lu":
										uci = 0;
										break;
								}
								if (isNot)
								{
									next = FA.Set(ToPairs(CharacterClasses.UnicodeCategories[uci]), accept, compact);
								}
								else
									next = FA.Set(ToPairs(CharacterClasses.NotUnicodeCategories[uci]), accept, compact);
								break;
							case 'd':
								next = FA.Set(ToPairs(CharacterClasses.digit), accept,compact);
								pc.Advance();
								break;
							case 'D':
								next = FA.Set(NotRanges(CharacterClasses.digit), accept,compact);
								pc.Advance();
								break;

							case 's':
								next = FA.Set(ToPairs(CharacterClasses.space), accept,compact);
								pc.Advance();
								break;
							case 'S':
								next = FA.Set(NotRanges(CharacterClasses.space), accept,compact);
								pc.Advance();
								break;
							case 'w':
								next = FA.Set(ToPairs(CharacterClasses.word), accept,compact);
								pc.Advance();
								break;
							case 'W':
								next = FA.Set(NotRanges(CharacterClasses.word), accept,compact);
								pc.Advance();
								break;
							default:
								if (-1 != (ich = _ParseEscapePart(pc)))
								{
									next = FA.Literal(new int[] { ich }, accept,compact);

								}
								else
								{
									pc.Expecting(); // throw an error
									return null; // doesn't execute
								}
								break;
						}
						next = _ParseModifier(next, pc, accept,compact);
						if (null != result)
						{
							result = FA.Concat(new FA[] { result, next }, accept,compact);
						}
						else
							result = next;
						break;
					case ')':
						//result = result.ToMinimized();
						return result;
					case '(':
						pc.Advance();
						pc.Expecting();
						next = Parse(pc, accept,compact);
						pc.Expecting(')');
						pc.Advance();
						next = _ParseModifier(next, pc, accept,compact);
						if (null == result)
							result = next;
						else
						{
							result = FA.Concat(new FA[] { result, next }, accept,compact);
						}
						break;
					case '|':
						if (-1 != pc.Advance())
						{
							next = Parse(pc, accept,compact);
							result = FA.Or(new FA[] { result, next }, accept,compact);
						}
						else
						{
							result = FA.Optional(result, accept,compact);
						}
						break;
					case '[':
						var seti = _ParseSet(pc);
						IEnumerable<KeyValuePair<int, int>> set;
						if (seti.Key)
							set = NotRanges(seti.Value);
						else
							set = ToPairs(seti.Value);
						next = FA.Set(set, accept);
						next = _ParseModifier(next, pc, accept, compact);

						if (null == result)
							result = next;
						else
						{
							result = FA.Concat(new FA[] { result, next }, accept, compact);

						}
						break;
					default:
						ich = pc.Current;
						next = FA.Literal(new int[] { ich }, accept,compact);
						pc.Advance();
						next = _ParseModifier(next, pc, accept,compact);
						if (null == result)
							result = next;
						else
						{
							result = FA.Concat(new FA[] { result, next }, accept,compact);
						}
						break;
				}
			}
		}

		static KeyValuePair<bool, int[]> _ParseSet(LexContext pc)
		{
			var result = new List<int>();
			pc.EnsureStarted();
			pc.Expecting('[');
			pc.Advance();
			pc.Expecting();
			var isNot = false;
			if ('^' == pc.Current)
			{
				isNot = true;
				pc.Advance();
				pc.Expecting();
			}
			var firstRead = true;
			int firstChar = '\0';
			var readFirstChar = false;
			var wantRange = false;
			while (-1 != pc.Current && (firstRead || ']' != pc.Current))
			{
				if (!wantRange)
				{
					// can be a single char,
					// a range
					// or a named character class
					if ('[' == pc.Current) // named char class
					{
						pc.Advance();
						pc.Expecting();
						if (':' != pc.Current)
						{
							firstChar = '[';
							readFirstChar = true;
						}
						else
						{
							pc.Advance();
							pc.Expecting();
							var ll = pc.CaptureBuffer.Length;
							if (!pc.TryReadUntil(':', false))
								throw new ExpectingException("Expecting character class", pc.Line, pc.Column, pc.Position, pc.FileOrUrl);
							pc.Expecting(':');
							pc.Advance();
							pc.Expecting(']');
							pc.Advance();
							var cls = pc.GetCapture(ll);
							int[] ranges;
							if (!CharacterClasses.Known.TryGetValue(cls, out ranges))
								throw new ExpectingException("Unknown character class \"" + cls + "\" specified", pc.Line, pc.Column, pc.Position, pc.FileOrUrl);
							result.AddRange(ranges);
							readFirstChar = false;
							wantRange = false;
							firstRead = false;
							continue;
						}
					}
					if (!readFirstChar)
					{
						if ('\\' == pc.Current)
						{
							pc.Advance();
							firstChar = _ParseRangeEscapePart(pc);
						}
						else
						{
							firstChar = pc.Current;
							pc.Advance();
							pc.Expecting();
						}
						readFirstChar = true;

					}
					else
					{
						if ('-' == pc.Current)
						{
							pc.Advance();
							pc.Expecting();
							wantRange = true;
						}
						else
						{
							result.Add(firstChar);
							result.Add(firstChar);
							readFirstChar = false;
						}
					}
					firstRead = false;
				}
				else
				{
					if ('\\' != pc.Current)
					{
						var ch = pc.Current;
						pc.Advance();
						pc.Expecting();
						result.Add(firstChar);
						result.Add(ch);
					}
					else
					{
						result.Add(firstChar);
						pc.Advance();
						result.Add(_ParseRangeEscapePart(pc));
					}
					wantRange = false;
					readFirstChar = false;
				}

			}
			if (readFirstChar)
			{
				result.Add(firstChar);
				result.Add(firstChar);
				if (wantRange)
				{
					result.Add('-');
					result.Add('-');
				}
			}
			pc.Expecting(']');
			pc.Advance();
			return new KeyValuePair<bool, int[]>(isNot, result.ToArray());
		}
		
		static FA _ParseModifier(FA expr, LexContext pc, int accept, bool compact)
		{
			var line = pc.Line;
			var column = pc.Column;
			var position = pc.Position;
			switch (pc.Current)
			{
				case '*':
					expr = Repeat(expr, 0, 0, accept,compact);
					pc.Advance();
					break;
				case '+':
					expr = Repeat(expr, 1, 0, accept,compact);
					pc.Advance();
					break;
				case '?':
					expr = Optional(expr, accept,compact);
					pc.Advance();
					break;
				case '{':
					pc.Advance();
					pc.TrySkipWhiteSpace();
					pc.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ',', '}');
					var min = -1;
					var max = -1;
					if (',' != pc.Current && '}' != pc.Current)
					{
						var l = pc.CaptureBuffer.Length;
						pc.TryReadDigits();
						min = int.Parse(pc.GetCapture(l));
						pc.TrySkipWhiteSpace();
					}
					if (',' == pc.Current)
					{
						pc.Advance();
						pc.TrySkipWhiteSpace();
						pc.Expecting('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '}');
						if ('}' != pc.Current)
						{
							var l = pc.CaptureBuffer.Length;
							pc.TryReadDigits();
							max = int.Parse(pc.GetCapture(l));
							pc.TrySkipWhiteSpace();
						}
					}
					else { max = min; }
					pc.Expecting('}');
					pc.Advance();
					expr = Repeat(expr, min, max, accept,compact);
					break;
			}
			return expr;
		}
		static byte _FromHexChar(int hex)
		{
			if (':' > hex && '/' < hex)
				return (byte)(hex - '0');
			if ('G' > hex && '@' < hex)
				return (byte)(hex - '7'); // 'A'-10
			if ('g' > hex && '`' < hex)
				return (byte)(hex - 'W'); // 'a'-10
			throw new ArgumentException("The value was not hex.", "hex");
		}
		static bool _IsHexChar(int hex)
		{
			if (':' > hex && '/' < hex)
				return true;
			if ('G' > hex && '@' < hex)
				return true;
			if ('g' > hex && '`' < hex)
				return true;
			return false;
		}
		// return type is either char or ranges. this is kind of a union return type.
		static int _ParseEscapePart(LexContext pc)
		{
			if (-1 == pc.Current) return -1;
			switch (pc.Current)
			{
				case 'f':
					pc.Advance();
					return '\f';
				case 'v':
					pc.Advance();
					return '\v';
				case 't':
					pc.Advance();
					return '\t';
				case 'n':
					pc.Advance();
					return '\n';
				case 'r':
					pc.Advance();
					return '\r';
				case 'x':
					if (-1 == pc.Advance() || !_IsHexChar(pc.Current))
						return 'x';
					byte b = _FromHexChar(pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar(pc.Current))
						return unchecked(b);
					b <<= 4;
					b |= _FromHexChar(pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar(pc.Current))
						return unchecked(b);
					b <<= 4;
					b |= _FromHexChar(pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar(pc.Current))
						return unchecked(b);
					b <<= 4;
					b |= _FromHexChar(pc.Current);
					return unchecked(b);
				case 'u':
					if (-1 == pc.Advance())
						return 'u';
					ushort u = _FromHexChar(pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked(u);
					u |= _FromHexChar(pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked(u);
					u |= _FromHexChar(pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked(u);
					u |= _FromHexChar(pc.Current);
					return unchecked(u);
				default:
					int i = pc.Current;
					pc.Advance();
					return i;
			}
		}
		static int _ParseRangeEscapePart(LexContext pc)
		{
			if (-1 == pc.Current)
				return -1;
			switch (pc.Current)
			{
				case '0':
					pc.Advance();
					return '\0';
				case 'f':
					pc.Advance();
					return '\f';
				case 'v':
					pc.Advance();
					return '\v';
				case 't':
					pc.Advance();
					return '\t';
				case 'n':
					pc.Advance();
					return '\n';
				case 'r':
					pc.Advance();
					return '\r';
				case 'x':
					if (-1 == pc.Advance() || !_IsHexChar(pc.Current))
						return 'x';
					byte b = _FromHexChar(pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar(pc.Current))
						return unchecked(b);
					b <<= 4;
					b |= _FromHexChar(pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar(pc.Current))
						return unchecked(b);
					b <<= 4;
					b |= _FromHexChar(pc.Current);
					if (-1 == pc.Advance() || !_IsHexChar(pc.Current))
						return unchecked(b);
					b <<= 4;
					b |= _FromHexChar(pc.Current);
					return unchecked(b);
				case 'u':
					if (-1 == pc.Advance())
						return 'u';
					ushort u = _FromHexChar(pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked(u);
					u |= _FromHexChar(pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked(u);
					u |= _FromHexChar(pc.Current);
					u <<= 4;
					if (-1 == pc.Advance())
						return unchecked(u);
					u |= _FromHexChar(pc.Current);
					return unchecked(u);
				default:
					int i = pc.Current;
					pc.Advance();
					return i;
			}
		}
		/// <summary>
		/// Turns packed ranges into unpacked ranges
		/// </summary>
		/// <param name="packedRanges">The ranges to unpack</param>
		/// <returns>The unpacked ranges</returns>
		static internal KeyValuePair<int, int>[] ToPairs(int[] packedRanges)
		{
			var result = new KeyValuePair<int, int>[packedRanges.Length / 2];
			for (var i = 0; i < result.Length; ++i)
			{
				var j = i * 2;
				result[i] = new KeyValuePair<int, int>(packedRanges[j], packedRanges[j + 1]);
			}
			return result;
		}
		/// <summary>
		/// Packs a series of ranges
		/// </summary>
		/// <param name="pairs">The ranges to pack</param>
		/// <returns>The packed ranges</returns>
		static internal int[] FromPairs(IList<KeyValuePair<int, int>> pairs)
		{
			var result = new int[pairs.Count * 2];
			for (int ic = pairs.Count, i = 0; i < ic; ++i)
			{
				var pair = pairs[i];
				var j = i * 2;
				result[j] = pair.Key;
				result[j + 1] = pair.Value;
			}
			return result;
		}
		/// <summary>
		/// Inverts a set of packed ranges
		/// </summary>
		/// <param name="ranges">The packed ranges to invert</param>
		/// <returns>The inverted unpacked</returns>
		static internal IList<KeyValuePair<int, int>> NotRanges(int[] ranges)
		{
			return new List<KeyValuePair<int, int>>(NotRanges(ToPairs(ranges)));
		}
		/// <summary>
		/// Inverts a set of unpacked ranges
		/// </summary>
		/// <param name="ranges">The ranges to invert</param>
		/// <returns>The inverted ranges</returns>
		static internal IEnumerable<KeyValuePair<int, int>> NotRanges(IEnumerable<KeyValuePair<int, int>> ranges)
		{
			// expects ranges to be normalized
			var last = 0x10ffff;
			using (var e = ranges.GetEnumerator())
			{
				if (!e.MoveNext())
				{
					yield return new KeyValuePair<int, int>(0x0, 0x10ffff);
					yield break;
				}
				if (e.Current.Key > 0)
				{
					yield return new KeyValuePair<int, int>(0, unchecked(e.Current.Key - 1));
					last = e.Current.Value;
					if (0x10ffff <= last)
						yield break;
				} else if(e.Current.Key==0) {
					last = e.Current.Value;
					if (0x10ffff <= last)
						yield break;
				}
				while (e.MoveNext())
				{
					if (0x10ffff <= last)
						yield break;
					if (unchecked(last + 1) < e.Current.Key)
						yield return new KeyValuePair<int, int>(unchecked(last + 1), unchecked((e.Current.Key - 1)));
					last = e.Current.Value;
				}
				if (0x10ffff > last)
					yield return new KeyValuePair<int, int>(unchecked((last + 1)), 0x10ffff);

			}

		}
		/// <summary>
		/// Makes a machine that is a deterministic equivalent of this machine
		/// </summary>
		/// <param name="progress">The progress indicator instance</param>
		/// <returns>A deterministic version of this machine</returns>
		public FA ToDfa(IProgress<int> progress = null)
		{
			return _Determinize(this,progress);
		}
		/// <summary>
		/// Converts to a DFA and optimizes the result
		/// </summary>
		/// <param name="progress">The progress indicator instance</param>
		/// <returns>A deterministic optimized version of this machine</returns>
		public FA ToMinimized(IProgress<int> progress = null)
		{
			return _Minimize(this,progress);
		}
		/// <summary>
		/// For this machine, fills and sorts transitions such that any missing range now points to an empty non-accepting state
		/// </summary>
		public void Totalize()
		{
			Totalize(FillClosure());
		}
		
		static int _TransitionComparison(FATransition x, FATransition y)
		{
			var c = x.Min.CompareTo(y.Min); if (0 != c) return c; return x.Max.CompareTo(y.Max);
		}
		/// <summary>
		/// For this closure, fills and sorts transitions such that any missing range now points to an empty non-accepting state
		/// </summary>
		/// <param name="closure">The closure to totalize</param>
		public static void Totalize(IList<FA> closure)
		{
			var s = new FA();
			s.Transitions.Add(new FATransition(0, 0x10ffff, s));
			foreach (FA p in closure)
			{
				int maxi = 0;
				var sortedTrans = new List<FATransition>(p.Transitions);
				sortedTrans.Sort(_TransitionComparison);
				foreach (var t in sortedTrans)
				{
					if(t.Min==-1 && t.Max==-1)
					{
						continue;
					}
					if (t.Min > maxi)
					{
						p.Transitions.Add(new FATransition(maxi, (t.Min - 1), s));
					}

					if (t.Max + 1 > maxi)
					{
						maxi = t.Max + 1;
					}
				}

				if (maxi <= 0x10ffff)
				{
					p.Transitions.Add(new FATransition(maxi, 0x10ffff, s));
				}
			}
		}

		static FA _Minimize(FA a,IProgress<int> progress)
		{
			int prog = 0;
			if(progress!=null) { progress.Report(prog); }
			a = a.ToDfa(progress);
			var tr = a.Transitions;
			if (1 == tr.Count)
			{
				FATransition t = tr[0];
				if (t.To == a && t.Min == 0 && t.Max == 0x10ffff)
				{
					return a;
				}
			}

			a.Totalize();
			prog = 1;
			if (progress != null) { progress.Report(prog); }
			// Make arrays for numbered states and effective alphabet.
			var cl = a.FillClosure();
			var states = new FA[cl.Count];
			int number = 0;
			foreach (var q in cl)
			{
				states[number] = q;
				q.Tag = number;
				++number;
			}

			var pp = new List<int>();
			for (int ic = cl.Count, i = 0; i < ic; ++i)
			{
				var ffa = cl[i];
				pp.Add(0);
				foreach (var t in ffa.Transitions)
				{
					pp.Add(t.Min);
					if (t.Max < 0x10ffff)
					{
						pp.Add((t.Max + 1));
					}
				}
			}

			var sigma = new int[pp.Count];
			pp.CopyTo(sigma, 0);
			Array.Sort(sigma);

			// Initialize data structures.
			var reverse = new List<List<Queue<FA>>>();
			foreach (var s in states)
			{
				var v = new List<Queue<FA>>();
				_Init(v, sigma.Length);
				reverse.Add(v);
			}
			prog = 2;
			if (progress != null) { progress.Report(prog); }
			var reverseNonempty = new bool[states.Length, sigma.Length];

			var partition = new List<LinkedList<FA>>();
			_Init(partition, states.Length);
			prog = 3;
			if (progress != null) { progress.Report(prog); }
			var block = new int[states.Length];
			var active = new _FList[states.Length, sigma.Length];
			var active2 = new _FListNode[states.Length, sigma.Length];
			var pending = new Queue<KeyValuePair<int, int>>();
			var pending2 = new bool[sigma.Length, states.Length];
			var split = new List<FA>();
			var split2 = new bool[states.Length];
			var refine = new List<int>();
			var refine2 = new bool[states.Length];

			var splitblock = new List<List<FA>>();
			_Init(splitblock, states.Length);
			prog = 4;
			if (progress != null) { progress.Report(prog); }
			for (int q = 0; q < states.Length; q++)
			{
				splitblock[q] = new List<FA>();
				partition[q] = new LinkedList<FA>();
				for (int x = 0; x < sigma.Length; x++)
				{
					reverse[q][x] = new Queue<FA>();
					active[q, x] = new _FList();
				}
			}

			// Find initial partition and reverse edges.
			foreach (var qq in states)
			{
				int j = qq.IsAccepting ? 0 : 1;

				partition[j].AddLast(qq);
				block[qq.Tag] = j;
				for (int x = 0; x < sigma.Length; x++)
				{
					var y = sigma[x];
					var p = qq._Step(y);
					var pn = p.Tag;
					reverse[pn][x].Enqueue(qq);
					reverseNonempty[pn, x] = true;
				}
				++prog;
				if (progress != null) { progress.Report(prog); }
			}

			// Initialize active sets.
			for (int j = 0; j <= 1; j++)
			{
				for (int x = 0; x < sigma.Length; x++)
				{
					foreach (var qq in partition[j])
					{
						if (reverseNonempty[qq.Tag, x])
						{
							active2[qq.Tag, x] = active[j, x].Add(qq);
						}
					}
				}
				++prog;
				if (progress != null) { progress.Report(prog); }
			}

			// Initialize pending.
			for (int x = 0; x < sigma.Length; x++)
			{
				int a0 = active[0, x].Count;
				int a1 = active[1, x].Count;
				int j = a0 <= a1 ? 0 : 1;
				pending.Enqueue(new KeyValuePair<int,int>(j, x));
				pending2[x, j] = true;
			}

			// Process pending until fixed point.
			int k = 2;
			while (pending.Count > 0)
			{
				KeyValuePair<int,int> ip = pending.Dequeue();
				int p = ip.Key;
				int x = ip.Value;
				pending2[x, p] = false;

				// Find states that need to be split off their blocks.
				for (var m = active[p, x].First; m != null; m = m.Next)
				{
					foreach (var s in reverse[m.State.Tag][x])
					{
						if (!split2[s.Tag])
						{
							split2[s.Tag] = true;
							split.Add(s);
							int j = block[s.Tag];
							splitblock[j].Add(s);
							if (!refine2[j])
							{
								refine2[j] = true;
								refine.Add(j);
							}
						}
					}
				}
				++prog;
				if (progress != null) { progress.Report(prog); }
				// Refine blocks.
				foreach (int j in refine)
				{
					if (splitblock[j].Count < partition[j].Count)
					{
						LinkedList<FA> b1 = partition[j];
						LinkedList<FA> b2 = partition[k];
						foreach (var s in splitblock[j])
						{
							b1.Remove(s);
							b2.AddLast(s);
							block[s.Tag] = k;
							for (int c = 0; c < sigma.Length; c++)
							{
								_FListNode sn = active2[s.Tag, c];
								if (sn != null && sn.StateList == active[j, c])
								{
									sn.Remove();
									active2[s.Tag, c] = active[k, c].Add(s);
								}
							}
						}

						// Update pending.
						for (int c = 0; c < sigma.Length; c++)
						{
							int aj = active[j, c].Count;
							int ak = active[k, c].Count;
							if (!pending2[c, j] && 0 < aj && aj <= ak)
							{
								pending2[c, j] = true;
								pending.Enqueue(new KeyValuePair<int,int>(j, c));
							}
							else
							{
								pending2[c, k] = true;
								pending.Enqueue(new KeyValuePair<int,int>(k, c));
							}
						}

						k++;
					}

					foreach (var s in splitblock[j])
					{
						split2[s.Tag] = false;
					}

					refine2[j] = false;
					splitblock[j].Clear();
					++prog;
					if (progress != null) { progress.Report(prog); }
				}

				split.Clear();
				refine.Clear();
			}
			++prog;
			if (progress != null) { progress.Report(prog); }
			// Make a new state for each equivalence class, set initial state.
			var newstates = new FA[k];
			for (int n = 0; n < newstates.Length; n++)
			{
				var s = new FA();
				newstates[n] = s;
				foreach (var q in partition[n])
				{
					if (q == a)
					{
						a = s;
					}
					s.AcceptSymbol = q.AcceptSymbol;
					s.Tag = q.Tag; // Select representative.				
					q.Tag = n;
				}
				++prog;
				if (progress != null) { progress.Report(prog); }
			}

			// Build transitions and set acceptance.
			foreach (var s in newstates)
			{
				var st = states[s.Tag];
				s.AcceptSymbol = st.AcceptSymbol;
				foreach (var t in st.Transitions)
				{
					s.Transitions.Add(new FATransition(t.Min, t.Max, newstates[t.To.Tag]));
				}
				++prog;
				if (progress != null) { progress.Report(prog); }
			}
			// remove dead transitions
			foreach (var ffa in a.FillClosure())
			{
				var itrns = new List<FATransition>(ffa.Transitions);
				foreach (var trns in itrns)
				{
					var acc = trns.To.FillAcceptingStates();
					if (0 == acc.Count)
					{
						ffa.Transitions.Remove(trns);
					}
				}
			}
			return a;
		}
		FA _Step(int input)
		{
			for (int ic = Transitions.Count, i = 0; i < ic; ++i)
			{
				var t = Transitions[i];
				if (t.Min <= input && input <= t.Max)
					return t.To;

			}
			return null;
		}
		static void _Init<T>(IList<T> list, int count)
		{
			for (int i = 0; i < count; ++i)
			{
				list.Add(default(T));
			}
		}
		
		private sealed class _FList
		{
			public int Count { get; set; }

			public _FListNode First { get; set; }

			public _FListNode Last { get; set; }

			public _FListNode Add(FA q)
			{
				return new _FListNode(q, this);
			}
		}
		/// <summary>
		/// Creates a packed state table as a series of integers
		/// </summary>
		/// <returns>An integer array representing the machine</returns>
		public int[] ToArray()
		{
			var working = new List<int>();
			var closure = new List<FA>();
			FillClosure(closure);
			var stateIndices = new int[closure.Count];
			for (var i = 0; i < closure.Count; ++i)
			{
				var cfa = closure[i];
				stateIndices[i] = working.Count;
				// add the accept
				working.Add(cfa.IsAccepting ? cfa.AcceptSymbol : -1);
				var itrgp = cfa.FillInputTransitionRangesGroupedByState(true);
				// add the number of transitions
				working.Add(itrgp.Count);
				foreach (var itr in itrgp)
				{
					// We have to fill in the following after the fact
					// We don't have enough info here
					// for now just drop the state index as a placeholder
					working.Add(closure.IndexOf(itr.Key));
					// add the number of packed ranges
					working.Add(itr.Value.Length / 2);
					// add the packed ranges
					working.AddRange(itr.Value);

				}
			}
			var result = working.ToArray();
			var state = 0;
			while (state < result.Length)
			{
				state++;
				var tlen = result[state++];
				for (var i = 0; i < tlen; ++i)
				{
					// patch the destination
					result[state] = stateIndices[result[state]];
					++state;
					var prlen = result[state++];
					state += prlen * 2;
				}
			}
			return result;
		}
		/// <summary>
		/// Builds a state machine based on the packed state table
		/// </summary>
		/// <param name="fa">The state table to build from</param>
		/// <returns>A new machine that represents the given packed state table</returns>
		public static FA FromArray(int[] fa)
		{
			if (null == fa) return null;
			if (fa.Length == 0) return new FA();
			var si = 0;
			var states = new Dictionary<int, FA>();
			while (si < fa.Length)
			{
				var newfa = new FA();
				states.Add(si, newfa);
				newfa.AcceptSymbol = fa[si++];
				var tlen = fa[si++];
				for (var i = 0; i < tlen; ++i)
				{
					++si; // tto
					var prlen = fa[si++];
					si += prlen * 2;
				}
			}
			si = 0;
			var sid = 0;
			while (si < fa.Length)
			{
				var newfa = states[si];
				var acc = fa[si++];
				var tlen = fa[si++];
				for (var i = 0; i < tlen; ++i)
				{
					var tto = fa[si++];
					var to = states[tto];
					var prlen = fa[si++];
					for (var j = 0; j < prlen; ++j)
					{
						var pmin = fa[si++];
						var pmax = fa[si++];
						newfa.Transitions.Add(new FATransition(pmin, pmax, to));
					}
				}
				++sid;
			}
			return states[0];
		}

		private sealed class _FListNode
		{
			public _FListNode(FA q, _FList sl)
			{
				State = q;
				StateList = sl;
				if (sl.Count++ == 0)
				{
					sl.First = sl.Last = this;
				}
				else
				{
					sl.Last.Next = this;
					Prev = sl.Last;
					sl.Last = this;
				}
			}

			public _FListNode Next { get; private set; }

			private _FListNode Prev { get; set; }

			public _FList StateList { get; private set; }

			public FA State { get; private set; }

			public void Remove()
			{
				StateList.Count--;
				if (StateList.First == this)
				{
					StateList.First = Next;
				}
				else
				{
					Prev.Next = Next;
				}

				if (StateList.Last == this)
				{
					StateList.Last = Prev;
				}
				else
				{
					Next.Prev = Prev;
				}
			}
		}

		static FA _Determinize(FA fa,IProgress<int> progress)
		{
			int prog = 0;
			if (progress != null) { progress.Report(prog); }
			var p = new HashSet<int>();
			var closure = new List<FA>();
			fa.FillClosure(closure);
			for (int ic = closure.Count, i = 0; i < ic; ++i)
			{
				var ffa = closure[i];
				p.Add(0);				
				foreach (var t in ffa.Transitions)
				{
					if(t.Min==-1 && t.Max==-1)
					{
						continue;
					}
					p.Add(t.Min);
					if (t.Max < 0x10ffff)
					{
						p.Add((t.Max + 1));
					}
				}
			}

			var points = new int[p.Count];
			p.CopyTo(points, 0);
			Array.Sort(points);
			++prog;
			if (progress != null) { progress.Report(prog); }
			var sets = new Dictionary<_KeySet<FA>, _KeySet<FA>>();
			var working = new Queue<_KeySet<FA>>();
			var dfaMap = new Dictionary<_KeySet<FA>, FA>();
			var initial = new _KeySet<FA>();
			var epscl = new List<FA>();
			fa.FillEpsilonClosure(epscl);
			foreach (var efa in epscl)
			{
				initial.Add(efa);
			}
			
			sets.Add(initial, initial);
			working.Enqueue(initial);
			var result = new FA();
			result.FromStates = epscl.ToArray();
			foreach (var afa in initial)
			{
				if (afa.IsAccepting)
				{
					result.AcceptSymbol = afa.AcceptSymbol;
					break;
				}
			}
			++prog;
			if (progress != null) { progress.Report(prog); }
			dfaMap.Add(initial, result);
			while (working.Count > 0)
			{
				var s = working.Dequeue();
				FA dfa;
				dfaMap.TryGetValue(s, out dfa);
				foreach (FA q in s)
				{
					if (q.IsAccepting)
					{
						dfa.AcceptSymbol = q.AcceptSymbol;
						break;
					}
				}

				for (var i = 0; i < points.Length; i++)
				{
					var pnt = points[i];
					var set = new _KeySet<FA>();
					foreach (FA c in s)
					{
						var ecs = c.FillEpsilonClosure();
						foreach (var efa in ecs)
						{
							foreach (var trns in efa.Transitions)
							{
								if(trns.Min == -1 && trns.Max == -1)
								{
									continue;
								}
								if (trns.Min <= pnt && pnt <= trns.Max)
								{
									foreach (var eefa in trns.To.FillEpsilonClosure())
									{
										set.Add(eefa);
									}
								}
							}
						}
					}
					if (!sets.ContainsKey(set))
					{
						sets.Add(set, set);
						working.Enqueue(set);
						var newfa = new FA();
						dfaMap.Add(set, newfa);
						var fas = new List<FA>(set);
						newfa.FromStates = fas.ToArray();
					}

					FA dst;
					dfaMap.TryGetValue(set, out dst);
					int first = pnt;
					int last;
					if (i + 1 < points.Length)
						last = (points[i + 1] - 1);
					else
						last = 0x10ffff;
					dfa.Transitions.Add(new FATransition(first, last, dst));
					++prog;
					if (progress != null) { progress.Report(prog); }
				}
				++prog;
				if (progress != null) { progress.Report(prog); }

			}
			// remove dead transitions
			foreach (var ffa in result.FillClosure())
			{
				var itrns = new List<FATransition>(ffa.Transitions);
				foreach (var trns in itrns)
				{
					var acc = trns.To.FillAcceptingStates();
					if (0 == acc.Count)
					{
						ffa.Transitions.Remove(trns);
					}
				}
				++prog;
				if (progress != null) { progress.Report(prog); }
			}
			++prog;
			if (progress != null) { progress.Report(prog); }
			return result;
		}
		/// <summary>
		/// Indicates whether or not the collection of states contains an accepting state
		/// </summary>
		/// <param name="states">The states to check</param>
		/// <returns>True if one or more of the states is accepting, otherwise false</returns>
		public static bool HasAcceptingState(IEnumerable<FA> states)
		{
			foreach (var state in states)
			{
				if (state.IsAccepting) return true;
			}
			return false;
		}
		/// <summary>
		/// Indicates whether or not the collection of states contains an accepting state
		/// </summary>
		/// <param name="states">The states to check</param>
		/// <returns>True if one or more of the states is accepting, otherwise false</returns>
		public static int GetFirstAcceptSymbol(IEnumerable<FA> states)
		{
			foreach (var state in states)
			{
				if (state.IsAccepting) return state.AcceptSymbol;
			}
			return -1;
		}
		/// <summary>
		/// Fills a list with all of the new states after moving from a given set of states along a given input. (NFA-move)
		/// </summary>
		/// <param name="states">The current states</param>
		/// <param name="codepoint">The codepoint to move on</param>
		/// <param name="result">A list to hold the next states. If null, one will be created.</param>
		/// <returns>The list of next states</returns>
		public static IList<FA> FillMove(IEnumerable<FA> states, int codepoint, IList<FA> result = null)
		{
			if (result == null) result = new List<FA>();
			foreach (var state in states)
			{
				for (int i = 0; i < state.Transitions.Count; ++i)
				{
					var fat = state.Transitions[i];
					if(fat.Min == -1 && fat.Max == -1)
					{
						continue;
					}
					if(fat.Min<=codepoint && codepoint<=fat.Max)
					{
						if (!result.Contains(fat.To))
						{
							result.Add(fat.To);
						}
						
					}
				}
				
			}
			return FillEpsilonClosure(result,null);
		}
		/// <summary>
		/// Retrieves a transition index given a specified UTF32 codepoint
		/// </summary>
		/// <param name="codepoint">The codepoint</param>
		/// <returns>The index of the matching transition or a negative number if no match was found.</returns>
		public int FindTransitionIndex(int codepoint)
		{
			for (var i = 0; i < Transitions.Count; ++i)
			{
				var t = Transitions[i];
				if(t.Min==-1 && t.Max==-1)
				{
					continue;
				}
				if (t.Min > codepoint)
				{
					return -1;
				}
				if (t.Max >= codepoint)
				{
					return i;
				}
			}
			return -1;
		}
		/// <summary>
		/// Retrieves all transition indices given a specified UTF32 codepoint
		/// </summary>
		/// <param name="codepoint">The codepoint</param>
		/// <returns>The indices of the matching transitions or empty if no match was found.</returns>
		public int[] FindTransitionIndices(int codepoint)
		{
			var result = new List<int>(Transitions.Count);
			for (var i = 0; i < Transitions.Count; ++i)
			{
				var t = Transitions[i];
				if (t.Min == -1 && t.Max == -1)
				{
					result.Add(i);
				}
				else if(t.Min <= codepoint && t.Max >= codepoint)
				{
					result.Add(i);
				}
			}
			return result.ToArray();
		}
		/// <summary>
		/// Returns the next state
		/// </summary>
		/// <param name="codepoint">The codepoint to move on</param>
		/// <returns>The next state, or null if there was no valid move.</returns>
		/// <remarks>This machine must be a DFA or this will error. Use FillMove() to work with any (slower).</remarks>
		public FA Move(int codepoint)
		{
			if(!IsDeterministic)
			{
				throw new InvalidOperationException("The state machine must be deterministic");
			}
			var i = FindTransitionIndex(codepoint);
			if (-1 < i)
			{
				return Transitions[i].To;
			}
			return null;
		}

		protected override int MatchImpl(LexContext lc)
		{
			lc.EnsureStarted();
			if (IsDeterministic)
			{
				var state = this;
				while (true)
				{
					var next = state.Move(lc.Current);
					if (null == next)
					{
						if (state.IsAccepting)
						{
							return lc.Current == LexContext.EndOfInput?state.AcceptSymbol:-1;
						}
						return -1;
					}
					lc.Advance();
					state = next;
					if (lc.Current == LexContext.EndOfInput)
					{
						return state.AcceptSymbol;
					}
				}
			}
			else
			{
				IList<FA> states = new List<FA>();
				states.Add(this);
				while (true)
				{
					states = FillEpsilonClosure(states,null);
					var next = FA.FillMove(states, lc.Current);
					if (next.Count == 0)
					{
						var acc = GetFirstAcceptSymbol(states);
						if (acc>-1)
						{
							return lc.Current == LexContext.EndOfInput?acc:-1;
						}
						return -1;
					}
					lc.Advance();
					states = next;
					if (lc.Current == LexContext.EndOfInput)
					{
						return GetFirstAcceptSymbol(states);
					}
				}
			}
		}
		/// <summary>
		/// Collapses epsilon transitions
		/// </summary>
		/// <param name="closure">The closure to collapse</param>
		public static void Compact(IList<FA> closure)
		{
			for(int i = 0; i < closure.Count;++i)
			{
				var fa = closure[i];
				if(fa.IsCompact) { continue; }
				var efas = fa.FillEpsilonClosure();
				var trans = new List<FATransition>();
				foreach(var efa in efas)
				{
					for(int j = 0;j<efa.Transitions.Count;++j)
					{
						var fat = efa.Transitions[j];
						if(fat.Min!=-1||fat.Max!=-1)
						{
							trans.Add(fat);
						}
					}
					if(efa.IsAccepting && !fa.IsAccepting)
					{
						fa.AcceptSymbol = efa.AcceptSymbol;
					}
				}
				fa.Transitions.Clear();
				fa.Transitions.AddRange(trans);
				fa.IsCompact = true;
			}
		}
		/// <summary>
		/// Collapses the epsilons on the current state machine.
		/// </summary>
		public void Compact()
		{
			Compact(FillClosure());
		}
		/// <summary>
		/// Indicates whether this machine will match the indicated text
		/// </summary>
		/// <param name="dfa">The DFA state table</param>
		/// <param name="text">The text</param>
		/// <returns>True if the passed in text was a match, otherwise false.</returns>
		public static int Match(int[] dfa, IEnumerable<char> text)
		{
			return Match(dfa, LexContext.Create(text));
		}
		/// <summary>
		/// Creates a lexer from a series of expressions
		/// </summary>
		/// <param name="tokens">The expressions to add. They typically each have different accept states.</param>
		/// <param name="makeDfa">Make the lexer a DFA. The first disjunction is converted to a DFA and the rest of the state machine is minimized.</param>
		/// <param name="compact">True to compact epsilons, otherwise false. Does nothing if <paramref name="makeDfa"/> is true.</param>
		/// <param name="progress">The progress converting to a lexer (DFA and minimization takes time). Only applies if <paramref name="makeDfa"/> is true.</param>
		/// <returns>The lexer machine</returns>
		public static FA ToLexer(IEnumerable<FA> tokens, bool makeDfa = true, bool compact = true,IProgress<int> progress = null)
		{
			var toks = new List<FA>(tokens);
			if (makeDfa)
			{
				for (int i = 0; i < toks.Count; i++)
				{
					toks[i] = toks[i].ToMinimized(progress);
				}
			}
			var result = new FA();
			for (int i = 0; i < toks.Count; i++)
			{
				result.AddEpsilon(toks[i],compact);
			}
			if (makeDfa)
			{
				return result.ToDfa(progress);
			} else
			{
				return result;
			}
		}
		
		/// <summary>
		/// Indicates whether this machine will match the indicated text
		/// </summary>
		/// <param name="dfa">The DFA state table</param>
		/// <param name="lc">A <see cref="LexContext"/> containing the text</param>
		/// <returns>True if the passed in text was a match, otherwise false.</returns>
		public static int Match(int[] dfa, LexContext lc)
		{
			lc.EnsureStarted();
			int si = 0;
			while (true)
			{
				// retrieve the accept id
				var acc = dfa[si++];
				if (lc.Current == LexContext.EndOfInput)
				{
					return acc;
				}
				// get the transitions count
				var trns = dfa[si++];
				var matched = false;
				for (var i = 0; i < trns; ++i)
				{
					// get the destination state
					var tto = dfa[si++];
					// get the packed range count
					var prlen = dfa[si++];
					for (var j = 0; j < prlen; ++j)
					{
						// get the min cp
						var min = dfa[si++];
						// get the max cp
						var max = dfa[si++];
						if (min > lc.Current)
						{
							si += (prlen - (j + 1)) * 2;
							break;
						}
						if (max >= lc.Current)
						{
							si = tto;
							matched = true;
							// break out of both loops
							goto next_state;
						}
					}
				}
				next_state:
				if (!matched)
				{
					// is the state accepting?
					if (acc != -1)
					{
						return lc.Current == LexContext.EndOfInput?acc:-1;
					}
					return -1;
				}
				lc.Advance();
				if (lc.Current == LexContext.EndOfInput)
				{
					// is the current state accepting
					return dfa[si];
				}
			}
		}
		/// <summary>
		/// Searches through text for the next occurance of a pattern matchable by this machine
		/// </summary>
		/// <param name="lc">The <see cref="LexContext"/> with the text to search</param>
		/// <returns>A series of <see cref="FAMatch"/> instances with the matches</returns>
		protected override FAMatch SearchImpl(LexContext lc)
		{
			long position = lc.Position;
			int line = lc.Line;
			int column = lc.Column;

			lc.EnsureStarted();
			if(lc.Current == LexContext.EndOfInput)
			{
				return FAMatch.Create(-1, null, 0, 0, 0);
			}
			if(IsDeterministic)
			{
				var state = this;
				while (true)
				{
					var next = state.Move(lc.Current);
					if(next!=null)
					{
						lc.Capture();
						lc.Advance();
						state = next;
					}
					else
					{
						if (state.IsAccepting)
						{
							return FAMatch.Create(state.AcceptSymbol, lc.CaptureBuffer.ToString(), position, line, column);
						}
						lc.Advance();
						lc.ClearCapture();
						position = lc.Position;
						line = lc.Line;
						column = lc.Column;
						state = this;
						if (lc.Current == LexContext.EndOfInput)
						{
							break;
						}
					}
				}
			} else
			{
				var initial = this.FillEpsilonClosure();
				var states = initial;
				while (true)
				{
					var next = FillMove(states, lc.Current);
					if (next != null && next.Count>0)
					{
						lc.Capture();
						states = FillEpsilonClosure(next, null);
						lc.Advance();
					}
					else
					{
						int acc = GetFirstAcceptSymbol(states);
						if (acc > -1)
						{
							return FAMatch.Create(acc, lc.CaptureBuffer.ToString(), position, line, column);
						}
						lc.Advance();
						lc.ClearCapture();
						position = lc.Position;
						line = lc.Line;
						column = lc.Column;
						states = initial;
						if(lc.Current == LexContext.EndOfInput)
						{
							break;
						}
					}
					
				}
			}
			return FAMatch.Create(-1, null, 0, 0, 0);
			
		}
		
		/// <summary>
		/// Searches through text for the next occurance of a pattern matchable by the indicated machine
		/// </summary>
		/// <param name="dfa">The DFA state table</param>
		/// <param name="lc">The <see cref="LexContext"/> with the text to search</param>
		/// <returns>A series of <see cref="FAMatch"/> instances with the matches</returns>
		public static IEnumerable<FAMatch> Search(int[] dfa, LexContext lc)
		{
			long position = lc.Position;
			int line = lc.Line;
			int column = lc.Column;

			lc.EnsureStarted();
			lc.ClearCapture();
			if (lc.Current == LexContext.EndOfInput)
			{
				yield break;
			}

			int si=0;
			while (true)
			{
				int acc = dfa[si++];
				var trns = dfa[si++];
				var moved = false;
				for(var i = 0;i<trns;++i)
				{
					var tto = dfa[si++];
					var prlen = dfa[si++];
					for (var j = 0; j < prlen; ++j)
					{
						var min = dfa[si++];
						var max = dfa[si++];
						if (min > lc.Current)
						{
							si += (prlen - (j + 1)) * 2;
							break;
						}
						if (max >= lc.Current)
						{
							moved = true;
							si = tto;
							goto next;
						}
					}
				}
			next:
				if(moved)
				{
					lc.Capture();
					lc.Advance();
				}
				else
				{
					if (acc!=-1)
					{
						yield return FAMatch.Create(acc, lc.CaptureBuffer.ToString(), position, line, column);
					} else
					{
						lc.Advance();
					}
					lc.ClearCapture();
					position = lc.Position;
					line = lc.Line;
					column = lc.Column;
					si = 0;
					if (lc.Current == LexContext.EndOfInput)
					{
						break;
					}
				}
			}
		}
		/// <summary>
		/// Searches through text for the next occurance of a pattern matchable by this machine
		/// </summary>
		/// <param name="dfa">The DFA state table</param>
		/// <param name="@string">The <see cref="IEnumerable{char}"/> with the text to search</param>
		/// <returns>A series of <see cref="FAMatch"/> instances with the matches</returns>
		public static IEnumerable<FAMatch> Search(int[] dfa, IEnumerable<char> @string)
		{
			foreach (var match in Search(dfa,LexContext.Create(@string)))
			{
				yield return match;
			}
		}
		/// <summary>
		/// Searches through text for the next occurance of a pattern matchable by this machine
		/// </summary>
		/// <param name="dfa">The DFA state table</param>
		/// <param name="reader">The <see cref="TextReader"/> with the text to search</param>
		/// <returns>A series of <see cref="FAMatch"/> instances with the matches</returns>
		public static IEnumerable<FAMatch> Search(int[] dfa, TextReader reader)
		{
			foreach (var match in Search(dfa,LexContext.CreateFrom(reader)))
			{
				yield return match;
			}
		}
		/// <summary>
		/// Converts a series of characters into a series of UTF-32 codepoints
		/// </summary>
		/// <param name="string">The series of characters to convert</param>
		/// <returns>The series of UTF-32 codepoints</returns>
		/// <exception cref="IOException">The characters had a sequence that was not valid unicode</exception>
		public static IEnumerable<int> ToUtf32(IEnumerable<char> @string)
		{
			int chh = -1;
			foreach (var ch in @string)
			{
				if (char.IsHighSurrogate(ch))
				{
					chh = ch;
					continue;
				}
				else
					chh = -1;
				if (-1 != chh)
				{
					if (!char.IsLowSurrogate(ch))
						throw new IOException("Unterminated Unicode surrogate pair found in string.");
					yield return char.ConvertToUtf32(unchecked((char)chh), ch);
					chh = -1;
					continue;
				}
				yield return ch;
			}
		}
		private sealed class _KeySet<T> : ISet<T>, IEquatable<_KeySet<T>>
		{
			HashSet<T> _inner;
			int _hashCode;
			public _KeySet(IEqualityComparer<T> comparer)
			{
				_inner = new HashSet<T>(comparer);
				_hashCode = 0;
			}
			public _KeySet()
			{
				_inner = new HashSet<T>();
				_hashCode = 0;
			}
			public int Count => _inner.Count;

			public bool IsReadOnly => true;

			// hack - we allow this method so the set can be filled
			public bool Add(T item)
			{
				if (null != item)
					_hashCode ^= item.GetHashCode();
				return _inner.Add(item);
			}
			bool ISet<T>.Add(T item)
			{
				_ThrowReadOnly();
				return false;
			}
			public void Clear()
			{
				_ThrowReadOnly();
			}

			public bool Contains(T item)
			{
				return _inner.Contains(item);
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				_inner.CopyTo(array, arrayIndex);
			}

			void ISet<T>.ExceptWith(IEnumerable<T> other)
			{
				_ThrowReadOnly();
			}

			public IEnumerator<T> GetEnumerator()
			{
				return _inner.GetEnumerator();
			}

			void ISet<T>.IntersectWith(IEnumerable<T> other)
			{
				_ThrowReadOnly();
			}

			public bool IsProperSubsetOf(IEnumerable<T> other)
			{
				return _inner.IsProperSubsetOf(other);
			}

			public bool IsProperSupersetOf(IEnumerable<T> other)
			{
				return _inner.IsProperSupersetOf(other);
			}

			public bool IsSubsetOf(IEnumerable<T> other)
			{
				return _inner.IsSubsetOf(other);
			}

			public bool IsSupersetOf(IEnumerable<T> other)
			{
				return _inner.IsSupersetOf(other);
			}

			public bool Overlaps(IEnumerable<T> other)
			{
				return _inner.Overlaps(other);
			}

			bool ICollection<T>.Remove(T item)
			{
				_ThrowReadOnly();
				return false;
			}

			public bool SetEquals(IEnumerable<T> other)
			{
				return _inner.SetEquals(other);
			}

			void ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
			{
				_ThrowReadOnly();
			}

			void ISet<T>.UnionWith(IEnumerable<T> other)
			{
				_ThrowReadOnly();
			}

			void ICollection<T>.Add(T item)
			{
				_ThrowReadOnly();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _inner.GetEnumerator();
			}
			static void _ThrowReadOnly()
			{
				throw new NotSupportedException("The set is read only");
			}
			public bool Equals(_KeySet<T> rhs)
			{
				if (ReferenceEquals(this, rhs))
					return true;
				if (ReferenceEquals(rhs, null))
					return false;
				if (rhs._hashCode != _hashCode)
					return false;
				var ic = _inner.Count;
				if (ic != rhs._inner.Count)
					return false;
				return _inner.SetEquals(rhs._inner);
			}
			public override int GetHashCode()
			{
				return _hashCode;
			}
		}
	}
}

