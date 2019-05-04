using System;
using System.Linq;
using System.Collections.Generic;
using Elements;
using Elements.Geometry;


namespace Bim42HyparQto
{
    /// <summary>
    /// The Structure of the building
    /// </summary>
    public class Structure
    {
        private Model _model;
        private BuildingDimensions _dim;
        private double _beam_height; //full height, including slab thickness
        private double _slab_height;
        private double _column_diameter;
        private double _column_offset;

        private FloorType _slabType;
        private StructuralFramingType _columnType;
        private StructuralFramingType _beamType;

        public Structure(Model model, BuildingDimensions dim)
        {
            _model = model;
            _dim = dim;

            // Set dimensions
            _beam_height = 0.6;
            _slab_height = 0.2;
            _column_diameter = 0.5;
            _column_offset = 0.5;

            //Create floor types
            List<MaterialLayer> slabMaterial = new List<MaterialLayer>() {
                new MaterialLayer(BuiltInMaterials.Concrete,_slab_height)
            };
            _slabType = new FloorType("slab", slabMaterial);

            //Create columns types
            Profile circularColumnProfile = new Profile(Polygon.Circle(_column_diameter / 2));
            _columnType = new StructuralFramingType("Circular Column", circularColumnProfile, BuiltInMaterials.Concrete);

            //Create beams types
            Profile beamProfile = new Profile(Polygon.Rectangle(_column_diameter, _beam_height - _slab_height));
            _beamType = new StructuralFramingType("Beam", beamProfile, BuiltInMaterials.Concrete);
        }

        public double BeamHeight { get { return _beam_height; } set { _beam_height = value; } }
        public double SlabHeight { get { return _slab_height; } set { _slab_height = value; } }
        public StructuralFramingType ColumnType { get { return _columnType; } set { _columnType = value; } }
        // public double ColumnDiameter { get { return _column_diameter; } set { _column_diameter = value; } }
        // public double ColumnOffset { get { return _column_offset; } set { _column_offset = value; } }

        public void CreateStructure(GridEx buildingGrid, double facadeThickness)
        {
            List<Cell> bottomCells = buildingGrid.BottomCells;
            CreateFraming(bottomCells, facadeThickness);

            List<Cell> topCells = buildingGrid.TopCells;
            CreateFraming(topCells, facadeThickness);

            // CreateSlab(buildingGrid,facadeThickness);
        }

        private void CreateSlab(GridEx buildingGrid, double facadeThickness)
        {
            Vector3[] buildingCell =new Vector3[4] {
                buildingGrid.Cells[0,0].Points[0],
                buildingGrid.Cells[0,buildingGrid.Cells.GetLength(1)-1].Points[1],
                buildingGrid.Cells[buildingGrid.Cells.GetLength(0)-1,buildingGrid.Cells.GetLength(1)-1].Points[2],
                buildingGrid.Cells[buildingGrid.Cells.GetLength(0)-1,0].Points[3]
            };
                        //Create a slab
            Polygon polygon = new Polygon(buildingCell);
            polygon = polygon.Offset(-facadeThickness)[0];
            Plane polygonPlane = polygon.Plane();
            Vector3 normal = polygonPlane.Normal;
            if (normal.Z < 0) { polygon = polygon.Reversed(); }
            Floor bottomFloor = new Floor(new Profile(polygon), _slabType, _dim.LevelHeight);
            _model.AddElement(bottomFloor);
        }

        private void CreateFraming(List<Cell> cells, double facadeThickness)
        {
            Vector3 beamOrigin = null;
            bool startingCell = true;

            for (int i = 0; i < cells.Count; i++)
            {
                //Create a beam each 3 module
                Math.DivRem(i, 3, out int remainer);
                if (remainer == 0)
                {
                    Cell currentCell = cells[i];
                    if (i != 0) { startingCell = false; }
                    beamOrigin = CreateFramingInCell(currentCell, beamOrigin, startingCell, facadeThickness);
                }
            }

            //Complete for the last module
            beamOrigin = CreateFramingInCell(cells[cells.Count - 1], beamOrigin, false, facadeThickness);
        }

        private Vector3 CreateFramingInCell(Cell currentCell, Vector3 beamOrigin, bool startingCell, double facadeThickness)
        {
            Vector3 startingPoint = new Vector3();
            Vector3 endingPoint = new Vector3();

            Line[] lines = currentCell.OuterLines;
            Vector3 towardInside = currentCell.TowardsInside[0];
            double offset = facadeThickness + _column_offset;
            Vector3 columnOffset = towardInside * offset;

            if (lines.Length == 2)
            {
                columnOffset = currentCell.TowardsInside[1] * offset + currentCell.TowardsInside[0] * offset;
                if (startingCell)
                {
                    startingPoint = lines[0].End;
                    towardInside = currentCell.TowardsInside[1];
                }
                else
                {
                    startingPoint = lines[0].End;
                    towardInside = towardInside.Negated();
                }
            }
            else
            {
                startingPoint = lines[0].Start;
            }

            double column_height = _dim.LevelHeight - _beam_height;
            Column circularColumn = new Column(startingPoint + columnOffset, column_height, _columnType, null, 0, 0);
            _model.AddElement(circularColumn);

            Vector3 beamElevation = new Vector3(0, 0, _dim.LevelHeight - _slab_height - (_beam_height - _slab_height) / 2);
            if (beamOrigin == null)
            {
                beamOrigin = startingPoint + columnOffset + beamElevation;
            }
            else
            {
                Line beamLine = new Line(beamOrigin, startingPoint + columnOffset + beamElevation);
                Beam beam = new Beam(beamLine, _beamType);
                beamOrigin = startingPoint + columnOffset + beamElevation;
                _model.AddElement(beam);
            }

            return beamOrigin;
        }

    }
}