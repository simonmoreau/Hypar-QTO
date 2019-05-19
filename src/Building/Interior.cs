using System;
using System.Linq;
using System.Collections.Generic;
using Elements;
using Elements.Geometry;


namespace Bim42HyparQto
{
    /// <summary>
    /// The properties of the spaces of the buidings
    /// </summary>
    public class Interior
    {
        private Model _model;
        private BuildingDimensions _dim;
        private double _ceiling_void_height;
        private double _ceiling_thickness;
        private double _raised_floor_void_height;
        private double _raised_floor_thickness;
        private double _headroom;
        private FloorType _raisedFloorType;
        private FloorType _ceillingType;
        private WallType _meetingRoomWallType;

        public Interior(Model model, BuildingDimensions dim)
        {
            _model = model;
            _dim = dim;
            _ceiling_void_height = 0.05;
            _ceiling_thickness = 0.05;
            _raised_floor_void_height = 0.07;
            _raised_floor_thickness = 0.03;
            _headroom = 2.7;

            List<MaterialLayer> raisedFloorMaterial = new List<MaterialLayer>() {
                new MaterialLayer(BuiltInMaterials.Wood,_raised_floor_thickness)
            };
            _raisedFloorType = new FloorType("Raised floor", raisedFloorMaterial);

            List<MaterialLayer> ceillingMaterial = new List<MaterialLayer>() {
                new MaterialLayer(BuiltInMaterials.Default,_ceiling_thickness)
            };

            _ceillingType = new FloorType("Ceilling", ceillingMaterial);

            //Create walls types
            List<MaterialLayer> meetingRoomMaterialLayers = new List<MaterialLayer>(){
                new MaterialLayer(BuiltInMaterials.Glass, 0.1)
            };
            _meetingRoomWallType = new WallType("Meeting Room Wall", meetingRoomMaterialLayers);

        }
        public double CeilingVoidHeight { get { return _ceiling_void_height; } set { _ceiling_void_height = value; } }
        public double CeilingThickness { get { return _ceiling_thickness; } set { _ceiling_thickness = value; } }
        public double RaisedFloorVoidHeight { get { return _raised_floor_void_height; } set { _raised_floor_void_height = value; } }
        public double RaisedFloorThickness { get { return _raised_floor_thickness; } set { _raised_floor_thickness = value; } }
        public double Headroom { get { return _headroom; } set { _headroom = value; } }

        //         public FloorType RaisedFloorType { get { return raisedFloorType; } }
        // public FloorType CeillingType { get { return ceillingType; } }

        // public WallType MeetingRoomWallType { get { return meetingRoomWallType; } }

        public void CreateInterior(GridEx buildingGrid, double facadeThickness)
        {
            CreateInteriorFlooring(buildingGrid, facadeThickness);
        }

        private void CreateInteriorFlooring(GridEx buildingGrid, double facadeThickness)
        {
            int dim0 = buildingGrid.Cells.GetLength(0) - 1;
            int dim1 = buildingGrid.Cells.GetLength(1) - 1;

            Vector3 facadeOffset = (buildingGrid.Cells[0,1].Points[1] - buildingGrid.Cells[0,0].Points[0]).Normalized() * facadeThickness;
            Vector3[] officeCellBottom = new Vector3[4] {
                buildingGrid.Cells[0,0].Points[0] + facadeOffset,
                buildingGrid.Cells[0,1].Points[1],
                buildingGrid.Cells[dim0,1].Points[2],
                buildingGrid.Cells[dim0,0].Points[3] + facadeOffset
            };

            CreateFloor(officeCellBottom, _ceillingType, _ceiling_thickness + _headroom + _raised_floor_thickness + _raised_floor_void_height);
            CreateFloor(officeCellBottom, _raisedFloorType, _raised_floor_thickness + _raised_floor_void_height);

            facadeOffset = (buildingGrid.Cells[0,dim1 -1].Points[0] - buildingGrid.Cells[0,dim1].Points[1]).Normalized() * facadeThickness;
            Vector3[] officeCellTop = new Vector3[4] {
                buildingGrid.Cells[0,dim1 -1].Points[0],
                buildingGrid.Cells[0,dim1].Points[1]  + facadeOffset,
                buildingGrid.Cells[dim0,dim1].Points[2] + facadeOffset,
                buildingGrid.Cells[dim0,dim1-1].Points[3]
            };

            CreateFloor(officeCellTop, _ceillingType, _ceiling_thickness + _headroom + _raised_floor_thickness + _raised_floor_void_height);
            CreateFloor(officeCellTop, _raisedFloorType, _raised_floor_thickness + _raised_floor_void_height);

        }

        private void CreateFloor(Vector3[] buildingCell, FloorType floorType, double elevation)
        {
            //Create a slab
            Polygon polygon = new Polygon(buildingCell);
            Plane polygonPlane = polygon.Plane();
            Vector3 normal = polygonPlane.Normal;
            if (normal.Z < 0) { polygon = polygon.Reversed(); }
            Floor bottomFloor = new Floor(polygon, floorType, elevation);
            _model.AddElement(bottomFloor);
        }

    }
}