using System;
using System.Linq;
using System.Collections.Generic;
using Elements;
using Elements.Geometry;


namespace Bim42HyparQto
{
    /// <summary>
    /// The Facade of the building
    /// </summary>
    public class Facade
    {
        private Model _model;
        private BuildingDimensions _dim;
        private Structure _structure;
        private Spaces _spaces;
        private double _facade_thickness;

        private WallType _facadeType;
        private StructuralFramingType _mullionType;

        public Facade(Model model, BuildingDimensions dim, Spaces spaces, Structure structure)
        {
            _model = model;
            _dim = dim;
            _spaces = spaces;
            _structure = structure;
            _facade_thickness = 0.4;

            //Create types
                        List<MaterialLayer> façadeMaterialLayers = new List<MaterialLayer>(){
                new MaterialLayer(BuiltInMaterials.Glass, _facade_thickness)
            };
            _facadeType = new WallType("façade", façadeMaterialLayers);

                        //Create mullion types
            Profile mullionProfile = new Profile(Polygon.Rectangle(0.03, 0.1));
            _mullionType = new StructuralFramingType("Mullion", mullionProfile, BuiltInMaterials.Steel);
        }

        public double FacadeThickness { get { return _facade_thickness; } set { _facade_thickness = value; } }

        public void CreateFaçades(GridEx buildingGrid)
        {
            Vector3 levelHeight = Vector3.ZAxis * _dim.LevelHeight;

            foreach (Cell outerCell in buildingGrid.OuterCells)
            {
                foreach (Line line in outerCell.GetExteriorLines())
                {
                    Vector3[] facadeCell = new Vector3[] {
                    line.Start,
                    line.Start + levelHeight,
                    line.End + levelHeight,
                    line.End
                    };

                    Plane facadeCellPlane = new Plane(line.Start, line.Start + levelHeight, line.End);
                    Vector3 towardInside = facadeCellPlane.Normal.Normalized();

                    CreateFacadeModule(facadeCell, towardInside);
                }
            }
        }

        private void CreateFacadeModule(Vector3[] facadeCell, Vector3 towardInside)
        {
            Vector3 innerVector = towardInside * _facade_thickness;
            Polygon cellPolygon = new Polygon(facadeCell);
            if (cellPolygon.Plane().Normal.Normalized().Equals(innerVector.Normalized()))
            {
                facadeCell = cellPolygon.Reversed().Vertices;
            }

            Vector3[] innerCell = facadeCell.Select(v => v + innerVector).ToArray();

            double[,] panelsDimensions = new double[4, 2] {
{0.15,0.35},
{0.15,0.25},
{0.05,0.05},
{0.15,0.65}
};

            Random rnd = new Random();
            int panelNumber = rnd.Next(0, 3);

            Transform panelTransform = new Transform(innerCell[0], innerCell[3] - innerCell[0], innerVector.Negated());
            double leftThickness = panelsDimensions[panelNumber, 0];
            double topThickness = _spaces.CeilingVoidHeight + _spaces.CeilingVoidHeight + _structure.BeamHeight;
            double rightThickness = panelsDimensions[panelNumber, 1];
            double bottomThickness = _spaces.RaisedFloorVoidHeight + _spaces.RaisedFloorThickness;
            Vector3[] glassCell = new Vector3[4];
            glassCell[0] = innerCell[0] + panelTransform.OfVector(new Vector3(leftThickness, (-1) * bottomThickness, 0));
            glassCell[1] = innerCell[1] + panelTransform.OfVector(new Vector3(leftThickness, topThickness, 0));
            glassCell[2] = innerCell[2] + panelTransform.OfVector(new Vector3((-1) * rightThickness, topThickness, 0));
            glassCell[3] = innerCell[3] + panelTransform.OfVector(new Vector3((-1) * rightThickness, (-1) * bottomThickness, 0));

            Polygon glassPolygon = new Polygon(glassCell);

            Material facadePanelMaterial = new Material("facadeMaterial", Colors.White, (float)0.1, (float)0.1, true);

            Panel glassPanel = new Panel(glassCell, BuiltInMaterials.Glass);
            _model.AddElement(glassPanel);

            Vector3 mullionStart = glassCell[0] + new Vector3(0, 0, 1);
            Vector3 mullionEnd = glassCell[3] + new Vector3(0, 0, 1);
            Beam mullion = new Beam(new Line(mullionStart, mullionEnd), _mullionType);
            _model.AddElement(mullion);


            for (int i = 0; i < 4; i++)
            {
                int j = i + 1;
                if (i == 3) { j = 0; }
                Panel panel = new Panel(new Vector3[] {
                innerCell[i],
                facadeCell[i],
                facadeCell[j],
                innerCell[j]
            }, facadePanelMaterial);
                _model.AddElement(panel);

                Panel sidePanel = new Panel(new Vector3[] {
                glassCell[i],
                facadeCell[i],
                facadeCell[j],
                glassCell[j]
            }, facadePanelMaterial);
                _model.AddElement(sidePanel);

                Panel innerPannel = new Panel(new Vector3[] {
                glassCell[i],
                innerCell[i],
                innerCell[j],
                glassCell[j]
            }, facadePanelMaterial);
                _model.AddElement(innerPannel);
            }

        }

        private void CreateGroundFacadeModule(Vector3[] facadeCell, Vector3 towardInside)
        {
            Vector3 innerVector = towardInside * (_facade_thickness - 0.03 / 2);
            Polygon cellPolygon = new Polygon(facadeCell);
            if (cellPolygon.Plane().Normal.Normalized().Equals(innerVector.Normalized()))
            {
                facadeCell = cellPolygon.Reversed().Vertices;
            }

            Vector3[] mullionCell = facadeCell.Select(v => v + innerVector).ToArray();

            Transform panelTransform = new Transform(mullionCell[0], mullionCell[3] - mullionCell[0], innerVector);



            double[] mullionsElevation = new double[4] {
                0.1/2,
                3,
                5-_structure.SlabHeight-_structure.BeamHeight-_spaces.CeilingVoidHeight-_spaces.CeilingThickness,
                5-0.1/2};

            for (int i = 0; i < mullionsElevation.Length; i++)
            {
                Vector3 left = mullionCell[0] + panelTransform.OfVector(new Vector3(0.05, mullionsElevation[i], 0));

                Vector3 right = mullionCell[3] + panelTransform.OfVector(new Vector3(-0.05, mullionsElevation[i], 0));

                Beam mullion = new Beam(new Line(left, right), _mullionType);
                _model.AddElement(mullion);

                if (i < mullionsElevation.Length - 1)
                {
                    Material facadePanelMaterial = BuiltInMaterials.Glass;
                    if (i == 2) { facadePanelMaterial = new Material("facadeMaterial", Colors.White, (float)0.1, (float)0.1, true); }

                    Vector3 leftDown = mullionCell[0] + panelTransform.OfVector(new Vector3(0.05, mullionsElevation[i] + 0.05, 0));
                    Vector3 rightDown = mullionCell[3] + panelTransform.OfVector(new Vector3(-0.05, mullionsElevation[i] + 0.05, 0));
                    Vector3 leftUp = mullionCell[0] + panelTransform.OfVector(new Vector3(0.05, mullionsElevation[i + 1] - 0.05, 0));
                    Vector3 rightUp = mullionCell[3] + panelTransform.OfVector(new Vector3(-0.05, mullionsElevation[i + 1] - 0.05, 0));
                    Panel innerPannel = new Panel(new Vector3[] {
                leftDown,
                leftUp,
                rightUp,
                rightDown
            }, facadePanelMaterial);
                    _model.AddElement(innerPannel);
                }

                Beam LeftMullion = new Beam(new Line(mullionCell[0], mullionCell[1]), _mullionType);
                _model.AddElement(LeftMullion);

            }
        }
    }
}