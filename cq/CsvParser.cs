using System;
using System.Collections.Generic;
using System.Linq;

namespace cq
{
    public static class CsvParser
    {
        private enum State
        {
            NewRecord,
            CarriageReturn,
            Delimited,
            NakedCell,
            QuotedCell,
            Eof,
            FormatError,
            QuoteEnd
        }

        private class Behaviour
        {
            public readonly int[] Triggers;
            public readonly State NextState;
            public readonly bool Record;
            public readonly bool PublishCell;
            public readonly bool PublishRow;

            public Behaviour(int[] trigger, State nextState, bool record = false, bool publishCell = false,
                bool publishRow = false)
            {
                Triggers = trigger;
                NextState = nextState;
                Record = record;
                PublishCell = publishCell;
                PublishRow = publishRow;
            }
        }

        private class BehaviourCollection
        {
            private readonly Behaviour _defaultBehaviour;
            private readonly Dictionary<int, Behaviour> _behaviours;

            public BehaviourCollection(IEnumerable<Behaviour> behaviours)
            {
                _behaviours = new Dictionary<int, Behaviour>();

                foreach (var behaviour in behaviours)
                {
                    if (behaviour.Triggers == null)
                        _defaultBehaviour = behaviour;
                    else
                    {
                        foreach (var behaviourTrigger in behaviour.Triggers)
                        {
                            _behaviours.Add(behaviourTrigger, behaviour);
                        }
                    }
                }
            }

            public Behaviour this[int index] => _behaviours.ContainsKey(index) ? _behaviours[index] : _defaultBehaviour;
        }

        private const int Eof = -1;
        private const int QuoteMark = '"';
        private const int Delimiter = ',';
        private const int Cr = '\r';
        private const int Lf = '\n';

        private static readonly Dictionary<State, BehaviourCollection> Automaton =
            new Dictionary<State, BehaviourCollection>()
            {
                {
                    State.NewRecord, new BehaviourCollection(new[]
                    {
                        new Behaviour(new int[] {QuoteMark}, State.QuotedCell),
                        new Behaviour(new int[] {Delimiter}, State.Delimited, publishCell: true),
                        new Behaviour(new int[] {Cr}, State.CarriageReturn, publishCell: true, publishRow: true),
                        new Behaviour(new int[] {Eof}, State.Eof, publishCell: false, publishRow: false),
                        new Behaviour(null, State.NakedCell, record: true),
                    })
                },
                {
                    State.Delimited, new BehaviourCollection(new[]
                    {
                        new Behaviour(new int[] {QuoteMark}, State.QuotedCell),
                        new Behaviour(new int[] {Delimiter}, State.Delimited, publishCell: true),
                        new Behaviour(new int[] {Cr}, State.CarriageReturn, publishCell: true, publishRow: false),
                        new Behaviour(new int[] {Eof}, State.Eof, publishCell: true, publishRow: true),
                        new Behaviour(null, State.NakedCell, record: true),
                    })
                },
                {
                    State.CarriageReturn, new BehaviourCollection(new[]
                    {
                        new Behaviour(new int[] {Lf}, State.NewRecord),
                        new Behaviour(null, State.FormatError),
                    })
                },
                {
                    State.NakedCell, new BehaviourCollection(new[]
                    {
                        new Behaviour(new int[] {Delimiter}, State.Delimited, publishCell: true),
                        new Behaviour(new int[] {Cr}, State.CarriageReturn, publishCell: true, publishRow: true),
                        new Behaviour(new int[] {QuoteMark, Lf}, State.FormatError),
                        new Behaviour(new int[] {Eof}, State.Eof, publishCell: true, publishRow: true),
                        new Behaviour(null, State.NakedCell, record: true),
                    })
                },
                {
                    State.QuotedCell, new BehaviourCollection(new[]
                    {
                        new Behaviour(new int[] {QuoteMark}, State.QuoteEnd),
                        new Behaviour(new int[] {Eof}, State.FormatError),
                        new Behaviour(null, State.QuotedCell, record: true),
                    })
                },
                {
                    State.QuoteEnd, new BehaviourCollection(new[]
                    {
                        new Behaviour(new int[] {QuoteMark}, State.QuotedCell, record: true),
                        new Behaviour(new int[] {Delimiter}, State.Delimited, publishCell: true),
                        new Behaviour(new int[] {Cr}, State.CarriageReturn, publishCell: true, publishRow: true),
                        new Behaviour(new int[] {Eof}, State.Eof, publishCell: true, publishRow: true),
                        new Behaviour(null, State.FormatError),
                    })
                },
            };

        public static IEnumerable<string[]> Parse(Func<int> supplier)
        {
            var row = new List<string>();
            var cell = string.Empty;
            var currentState = State.NewRecord;

            do
            {
                var c = supplier();
                var behaviour = Automaton[currentState][c];

                if (behaviour.Record)
                    cell += (char) c;
                if (behaviour.PublishCell)
                {
                    row.Add(cell);
                    cell = string.Empty;
                }

                if (behaviour.PublishRow)
                {
                    yield return row.ToArray();
                    row.Clear();
                }

                if (behaviour.NextState == State.FormatError)
                    throw new FormatException();

                currentState = behaviour.NextState;
            } while (currentState != State.Eof);
        }
    }
}