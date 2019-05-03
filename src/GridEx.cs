using Elements.Geometry;
using Elements.Geometry.Interfaces;
using System;
using System.Collections.Generic;

namespace Bim42HyparQto
{
    /// <summary>
    /// A grid comprised of rows and columns with each cell represented by a polyline.
    /// </summary>
    public class GridEx
    {
        private double[] _uDiv;
        private double[] _vDiv;
        private ICurve _bottom;
        private ICurve _top;
        private Cell[,] _cells;

        private List<Cell> _topCells = new List<Cell>();
        private List<Cell> _bottomCells = new List<Cell>();
        private List<Cell> _rightCells = new List<Cell>();
        private List<Cell> _leftCells = new List<Cell>();
        private List<Cell> _innerCells = new List<Cell>();
        private List<Cell> _outerCells = new List<Cell>();
        private Vector3[][] _pts;

        private double[] CalculateEqualDivisions(int n)
        {
            var uStep = 1.0 / (double)n;
            var result = new double[n + 1];
            for (var i = 0; i <= n; i++)
            {
                result[i] = uStep * i;
            }
            return result;
        }

        private double[] CalculateDivisions(double[] distances, double lenght)
        {
            int n = distances.Length;
            List<double> distancesNormalized = new List<double>();
            distancesNormalized.Add(0);
            double cumulatedDistances = 0;
            // Normalize distances
            for (var i = 0; i < n; i++)
            {
                cumulatedDistances = cumulatedDistances + distances[i];
                if (cumulatedDistances / lenght < 1)
                {
                    distancesNormalized.Add(cumulatedDistances / lenght);
                }
                else
                {
                    distancesNormalized.Add(1);
                    break;
                }
            }

            // Make sure we have a point at the end of the line
            if (distancesNormalized[distancesNormalized.Count - 1] != 1) { distancesNormalized.Add(1); }

            return distancesNormalized.ToArray();
        }

        private Vector3[][] CalculateGridPoints()
        {
            var pts = new Vector3[this._uDiv.Length][];
            var edge1 = this._bottom;
            var edge2 = this._top;

            for (var i = 0; i < _uDiv.Length; i++)
            {
                var u = _uDiv[i];

                var start = edge1.PointAt(u);
                var end = edge2.PointAt(u);
                var l = new Line(start, end);

                var col = new Vector3[_vDiv.Length];
                for (var j = 0; j < _vDiv.Length; j++)
                {
                    var v = _vDiv[j];
                    col[j] = l.PointAt(v);
                }
                pts[i] = col;
            }
            return pts;
        }

        private Cell[,] CalculateCells()
        {
            var results = new Cell[this._pts.GetLength(0) - 1, this._pts[0].Length - 1];

            for (var i = 0; i < this._pts.GetLength(0) - 1; i++)
            {
                var rowA = this._pts[i];
                var rowB = this._pts[i + 1];

                for (var j = 0; j < this._pts[i].Length - 1; j++)
                {
                    var a = rowA[j];
                    var b = rowA[j + 1];
                    var c = rowB[j + 1];
                    var d = rowB[j];
                    Cell cell = new Cell(new[] { a, b, c, d }, this._pts.GetLength(0) - 1, i, this._pts[i].Length - 1, j);
                    results[i, j] = cell;
                    if (i == 0) { _leftCells.Add(cell); }
                    if (i == this._pts.GetLength(0) - 2) { _rightCells.Add(cell); }
                    if (j == 0) { _bottomCells.Add(cell); }
                    if (j == this._pts[i].Length - 2) { _topCells.Add(cell); }
                    if (i != 0 && i != this._pts.GetLength(0) - 2 && j != 0 || j != this._pts[i].Length - 2) { _innerCells.Add(cell); }
                    if (i == 0 || i == this._pts.GetLength(0) - 2 || j == 0 || j == this._pts[i].Length - 2) { _outerCells.Add(cell); }
                }
            }

            return results;
        }

        /// <summary>
        /// Get all cells.
        /// </summary>
        /// <returns></returns>
        public Cell[,] Cells
        {
            get { return _cells; }
        }

        /// <summary>
        /// Get top cells.
        /// </summary>
        public List<Cell> TopCells
        {
            get { return _topCells; }
        }

        /// <summary>
        /// Get bottom cells.
        /// </summary>
        public List<Cell> BottomCells
        {
            get { return _bottomCells; }
        }

        /// <summary>
        /// Get left cells.
        /// </summary>
        public List<Cell> LeftCells
        {
            get { return _leftCells; }
        }

        /// <summary>
        /// Get right cells.
        /// </summary>
        public List<Cell> RightCells
        {
            get { return _rightCells; }
        }

        /// <summary>
        /// Get inner cells.
        /// </summary>
        public List<Cell> InnerCells
        {
            get { return _innerCells; }
        }

        /// <summary>
        /// Get outer cells.
        /// </summary>
        public List<Cell> OuterCells
        {
            get { return _outerCells; }
        }

        /// <summary>
        /// Construct a grid.
        /// </summary>
        /// <param name="bottom"></param>
        /// <param name="top"></param>
        /// <param name="uDivisions">The number of grid divisions in the u direction.</param>
        /// <param name="vDivisions">The number of grid divisions in the v direction.</param>
        public GridEx(Line bottom, Line top, int uDivisions = 1, int vDivisions = 1)
        {
            this._bottom = bottom;
            this._top = top;
            this._uDiv = CalculateEqualDivisions(uDivisions);
            this._vDiv = CalculateEqualDivisions(vDivisions);
            this._pts = CalculateGridPoints();
            this._cells = this.CalculateCells();
        }

        /// <summary>
        /// Construct a grid.
        /// </summary>
        /// <param name="bottom">The bottom edge of the Grid.</param>
        /// <param name="top">The top edge of the Grid.</param>
        /// <param name="uDistance">The distance along the u parameter at which points will be created.</param>
        /// <param name="vDistance">The distance along the v parameter at which points will be created.</param>
        public GridEx(Line bottom, Line top, double uDistance, double vDistance)
        {
            this._bottom = bottom;
            this._top = top;
            this._uDiv = CalculateEqualDivisions((int)Math.Ceiling(bottom.Length() / uDistance));
            this._vDiv = CalculateEqualDivisions((int)Math.Ceiling(top.Length() / vDistance));
            this._pts = CalculateGridPoints();
            this._cells = this.CalculateCells();
        }

        /// <summary>
        /// Construct a grid.
        /// </summary>
        /// <param name="bottom">The bottom edge of the Grid.</param>
        /// <param name="top">The top edge of the Grid.</param>
        /// <param name="uDistances">An array of distances along the u parameter at which points will be created.</param>
        /// <param name="vDistances">An array of distances along the v parameter at which points will be created.</param>
        public GridEx(Line bottom, Line top, double[] uDistances, double[] vDistances)
        {
            this._bottom = bottom;
            this._top = top;
            this._uDiv = CalculateDivisions(uDistances, top.Length());
            this._vDiv = CalculateDivisions(vDistances, (top.Start - bottom.Start).Length());
            this._pts = CalculateGridPoints();
            this._cells = this.CalculateCells();
        }
    }

    public class Cell
    {
        private Vector3[] _pts;
        private int _row;
        private int _column;
        private int _rowNum;
        private int _columnNum;

        /// <summary>
        /// Construct a cell.
        /// </summary>
        /// <param name="points">The points of the cell.</param>
        /// <param name="rowNum">The number of row in the parent grid.</param>
        /// <param name="row">The row of the cell.</param>
        /// <param name="columnNum">The number of columns in the parent grid.</param>
        /// <param name="column">The column of the cell.</param>
        public Cell(Vector3[] points, int rowNum, int row, int columnNum, int column)
        {
            _pts = points;
            _row = row;
            _rowNum = rowNum;
            _column = column;
            _columnNum = columnNum;
        }

        public Vector3[] Points
        {
            get
            {
                return _pts;
            }
        }

        public Line[] GetExteriorLines()
        {
            if (_row == 0 && _column == 0)
            {
                return new Line[2] { new Line(_pts[3], _pts[0]), new Line(_pts[0], _pts[1]) };
            }
            else if (_row == _rowNum - 1 && _column == 0)
            {
                return new Line[2] { new Line(_pts[2], _pts[3]), new Line(_pts[3], _pts[0]) };
            }
            else if (_row == 0 && _column == _columnNum - 1)
            {
                return new Line[2] { new Line(_pts[0], _pts[1]), new Line(_pts[1], _pts[2]) };
            }
            else if (_row == _rowNum - 1 && _column == _columnNum - 1)
            {
                return new Line[2] { new Line(_pts[1], _pts[2]), new Line(_pts[2], _pts[3]) };
            }
            else if (_row == 0)
            {
                return new Line[1] { new Line(_pts[0], _pts[1]) };
            }
            else if (_row == _rowNum - 1)
            {
                return new Line[1] { new Line(_pts[2], _pts[3]) };
            }
            else if (_column == 0)
            {
                return new Line[1] { new Line(_pts[3], _pts[0]) };
            }
            else if (_column == _columnNum - 1)
            {
                return new Line[1] { new Line(_pts[1], _pts[2]) };
            }
            else
            {
                return new Line[0];
            }
        }


        public Line[] GetInteriorLines()
        {
            if (_row == 0 && _column == 0)
            {
                return new Line[2] { new Line(_pts[1], _pts[2]), new Line(_pts[2], _pts[3]) };
            }
            else if (_row == _rowNum - 1 && _column == 0)
            {
                return new Line[2] { new Line(_pts[0], _pts[1]), new Line(_pts[1], _pts[2]) };
            }
            else if (_row == 0 && _column == _columnNum - 1)
            {
                return new Line[2] { new Line(_pts[2], _pts[3]), new Line(_pts[3], _pts[0]) };
            }
            else if (_row == _rowNum - 1 && _column == _columnNum - 1)
            {
                return new Line[2] { new Line(_pts[3], _pts[0]), new Line(_pts[0], _pts[1]) };
            }
            else if (_row == 0)
            {
                return new Line[3] { new Line(_pts[1], _pts[2]), new Line(_pts[2], _pts[3]), new Line(_pts[3], _pts[0]) };
            }
            else if (_row == _rowNum - 1)
            {
                return new Line[3] { new Line(_pts[3], _pts[0]), new Line(_pts[0], _pts[1]), new Line(_pts[1], _pts[2]) };
            }
            else if (_column == 0)
            {
                return new Line[3] { new Line(_pts[0], _pts[1]), new Line(_pts[1], _pts[2]), new Line(_pts[2], _pts[3]) };
            }
            else if (_column == _columnNum - 1)
            {
                return new Line[3] { new Line(_pts[2], _pts[3]), new Line(_pts[3], _pts[0]), new Line(_pts[0], _pts[1]) };
            }
            else
            {
                return new Line[4] { new Line(_pts[1], _pts[2]), new Line(_pts[2], _pts[3]), new Line(_pts[3], _pts[0]), new Line(_pts[0], _pts[1]) };
            }
        }

    }
}