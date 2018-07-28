using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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

        private const int Eof = -1;
        private const int QuoteMark = '"';
        private const int Delimiter = ',';
        private const int Cr = '\r';
        private const int Lf = '\n';

        private class Behaviour
        {
            public readonly State NextState;
            public readonly bool Record;
            public readonly bool PublishCell;
            public readonly bool PublishRow;

            public Behaviour(State nextState, bool record = false, bool publishCell = false,
                bool publishRow = false)
            {
                NextState = nextState;
                Record = record;
                PublishCell = publishCell;
                PublishRow = publishRow;
            }
        }

        private class BehaviourCollection
        {
            private readonly Behaviour _defaultBehaviour;
            private readonly Behaviour _eofBehaviour;
            private readonly Behaviour _quoteMarkBehaviour;
            private readonly Behaviour _delimiterBehaviour;
            private readonly Behaviour _crBehaviour;
            private readonly Behaviour _lfBehaviour;

            public BehaviourCollection(
                Behaviour defaultBehaviour,
                Behaviour eofBehaviour = null,
                Behaviour quoteBehaviour = null,
                Behaviour delimiterBehaviour = null,
                Behaviour crBehaviour = null,
                Behaviour lfBehaviour = null
            )
            {
                _defaultBehaviour = defaultBehaviour;
                _eofBehaviour = eofBehaviour ?? defaultBehaviour;
                _quoteMarkBehaviour = quoteBehaviour ?? defaultBehaviour;
                _delimiterBehaviour = delimiterBehaviour ?? defaultBehaviour;
                _crBehaviour = crBehaviour ?? defaultBehaviour;
                _lfBehaviour = lfBehaviour ?? defaultBehaviour;
            }

            public Behaviour Get(int input)
            {
                switch (input)
                {
                    case Eof: return _eofBehaviour;
                    case QuoteMark: return _quoteMarkBehaviour;
                    case Delimiter: return _delimiterBehaviour;
                    case Cr: return _crBehaviour;
                    case Lf: return _lfBehaviour;
                    default: return _defaultBehaviour;
                }
            }
        }

        private class NewAutomata
        {
            private readonly BehaviourCollection _newRecordBehaviours;
            private readonly BehaviourCollection _carriageReturnBehaviours;
            private readonly BehaviourCollection _delimitedBehaviours;
            private readonly BehaviourCollection _nakedCellBehaviours;
            private readonly BehaviourCollection _quotedCellBehaviours;
            private readonly BehaviourCollection _quoteEndBehaviours;

            public NewAutomata(
                BehaviourCollection newRecordBehaviours,
                BehaviourCollection carriageReturnBehaviours,
                BehaviourCollection delimitedBehaviours,
                BehaviourCollection nakedCellBehaviours,
                BehaviourCollection quotedCellBehaviours,
                BehaviourCollection quoteEndBehaviours
            )
            {
                _newRecordBehaviours = newRecordBehaviours;
                _carriageReturnBehaviours = carriageReturnBehaviours;
                _delimitedBehaviours = delimitedBehaviours;
                _nakedCellBehaviours = nakedCellBehaviours;
                _quotedCellBehaviours = quotedCellBehaviours;
                _quoteEndBehaviours = quoteEndBehaviours;
            }

            public Behaviour GetBehaviours(State currentState, int input)
            {
                return Get(currentState).Get(input);
            }

            private BehaviourCollection Get(State currentState)
            {
                switch (currentState)
                {
                    case State.NewRecord: return _newRecordBehaviours;
                    case State.CarriageReturn: return _carriageReturnBehaviours;
                    case State.Delimited: return _delimitedBehaviours;
                    case State.NakedCell: return _nakedCellBehaviours;
                    case State.QuotedCell: return _quotedCellBehaviours;
                    case State.QuoteEnd: return _quoteEndBehaviours;
                    default: throw new Exception(); // never comes here
                }
            }
        }

        [SuppressMessage("ReSharper", "ArgumentsStyleLiteral")] 
        private static readonly NewAutomata Automaton = new NewAutomata(
            newRecordBehaviours: new BehaviourCollection(
                quoteBehaviour: new Behaviour(State.QuotedCell),
                delimiterBehaviour: new Behaviour(State.Delimited, publishCell: true),
                crBehaviour: new Behaviour(State.CarriageReturn, publishCell: true, publishRow: true),
                eofBehaviour: new Behaviour(State.Eof),
                defaultBehaviour: new Behaviour(State.NakedCell, record: true)
            ),
            delimitedBehaviours: new BehaviourCollection(
                quoteBehaviour: new Behaviour(State.QuotedCell),
                delimiterBehaviour: new Behaviour(State.Delimited, publishCell: true),
                crBehaviour: new Behaviour(State.CarriageReturn, publishCell: true),
                eofBehaviour: new Behaviour(State.Eof, publishCell: true, publishRow: true),
                defaultBehaviour: new Behaviour(State.NakedCell, record: true)
            ),
            carriageReturnBehaviours: new BehaviourCollection(
                lfBehaviour: new Behaviour(State.NewRecord),
                defaultBehaviour: new Behaviour(State.FormatError)
            ),
            nakedCellBehaviours: new BehaviourCollection(
                delimiterBehaviour: new Behaviour(State.Delimited, publishCell: true),
                crBehaviour: new Behaviour(State.CarriageReturn, publishCell: true, publishRow: true),
                quoteBehaviour: new Behaviour(State.FormatError),
                lfBehaviour: new Behaviour(State.FormatError),
                eofBehaviour: new Behaviour(State.Eof, publishCell: true, publishRow: true),
                defaultBehaviour: new Behaviour(State.NakedCell, record: true)
            ),
            quotedCellBehaviours: new BehaviourCollection(
                quoteBehaviour: new Behaviour(State.QuoteEnd),
                eofBehaviour: new Behaviour(State.FormatError),
                defaultBehaviour: new Behaviour(State.QuotedCell, record: true)
            ),
            quoteEndBehaviours: new BehaviourCollection(
                quoteBehaviour: new Behaviour(State.QuotedCell, record: true),
                delimiterBehaviour: new Behaviour(State.Delimited, publishCell: true),
                crBehaviour: new Behaviour(State.CarriageReturn, publishCell: true, publishRow: true),
                eofBehaviour: new Behaviour(State.Eof, publishCell: true, publishRow: true),
                defaultBehaviour: new Behaviour(State.FormatError)
            ));

        public static IEnumerable<string[]> Parse(Func<int> supplier)
        {
            var row = new List<string>();
            var cell = new StringBuilder();
            var currentState = State.NewRecord;
            var rowCount = 1;

            do
            {
                var c = supplier();
                var behaviour = Automaton.GetBehaviours(currentState, c);

                if (behaviour.NextState == State.FormatError)
                {
                    throw new CsvFormatException(rowCount, row.Count + 1, cell.ToString());
                }

                if (behaviour.Record)
                {
                    cell.Append((char) c);
                }

                if (behaviour.PublishCell)
                {
                    row.Add(cell.ToString());
                    cell.Clear();
                }

                if (behaviour.PublishRow)
                {
                    yield return row.ToArray();
                    row.Clear();
                    rowCount++;
                }

                currentState = behaviour.NextState;
            } while (currentState != State.Eof);
        }
    }
}