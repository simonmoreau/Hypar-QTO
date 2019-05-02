using Elements;
using Elements.Geometry;
using System.Collections.Generic;

namespace Bim42HyparQto
{

    /// <summary>
    /// A singelton to regroup all building dimension
    /// </summary>
    public sealed class BuildingDimensions
    {
        private double _width;
        private double _lenght;
        private int _module_number;
        private double _core_width;
        private double _office_space_width;
        private double _corridor_width;
        private FacadeDimensions _facadeDimensions;
        private LevelDimensions _levelDimensions;
        private Types _types;

        private static readonly BuildingDimensions instance = new BuildingDimensions();

        static BuildingDimensions()
        {
        }

        private BuildingDimensions()
        {
            _facadeDimensions = FacadeDimensions.Instance;
            _levelDimensions = LevelDimensions.Instance;
            _module_number = 20;
            _width = 18.5;
            _core_width = 5;
            _corridor_width = 1.5;
            _lenght = _facadeDimensions.ModuleLenght * _module_number;
            _office_space_width = (_width - _core_width) / 2;
            _types = new Types(this);
        }

        public static BuildingDimensions Instance
        {
            get
            {
                return instance;
            }
        }

        public FacadeDimensions FacadeDimensions { get { return _facadeDimensions; } }
        public LevelDimensions LevelDimensions { get { return _levelDimensions; } }
        public Types Types { get { return _types; } }
        public int ModuleNumber { get { return _module_number; } set { _module_number = value; } }
        public double Width { get { return _width; } set { _width = value; } }
        public double CoreWidth { get { return _core_width; } set { _core_width = value; } }
        public double OfficeSpaceWidth { get { return _office_space_width; } set { _office_space_width = value; } }
        public double CorridorWidth { get { return _corridor_width; } set { _corridor_width = value; } }
        public double Lenght { get { return _lenght; } set { _lenght = value; } }
    }

    public class Types
    {
        private FloorType slabType;
        private FloorType raisedFloorType;
        private FloorType ceillingType;
        private StructuralFramingType columnType;
        private StructuralFramingType beamType;
        private StructuralFramingType mullionType;
        private WallType meetingRoomWallType;
        private WallType facadeType;

        public Types(BuildingDimensions dim)
        {
            //Create material
            Material concrete = new Material("Concrete", Elements.Geometry.Colors.Gray);

            //Create floor types
            List<MaterialLayer> slabMaterial = new List<MaterialLayer>() {
                new MaterialLayer(BuiltInMaterials.Concrete,dim.LevelDimensions.StructuralDimensions.SlabHeight)
            };
            slabType = new FloorType("slab", slabMaterial);

            List<MaterialLayer> raisedFloorMaterial = new List<MaterialLayer>() {
                new MaterialLayer(BuiltInMaterials.Wood,dim.LevelDimensions.RaisedFloorThickness)
            };
            raisedFloorType = new FloorType("Raised floor", raisedFloorMaterial);

            List<MaterialLayer> ceillingMaterial = new List<MaterialLayer>() {
                new MaterialLayer(BuiltInMaterials.Default,dim.LevelDimensions.CeilingThickness)
            };

            ceillingType = new FloorType("Ceilling", ceillingMaterial);

            //Create columns types
            Profile circularColumnProfile = new Profile(Polygon.Circle(dim.LevelDimensions.StructuralDimensions.ColumnDiameter / 2));
            columnType = new StructuralFramingType("Circular Column", circularColumnProfile, BuiltInMaterials.Concrete);

            //Create beams types
            Profile beamProfile = new Profile(Polygon.Rectangle(dim.LevelDimensions.StructuralDimensions.ColumnDiameter, dim.LevelDimensions.StructuralDimensions.BeamHeight - dim.LevelDimensions.StructuralDimensions.SlabHeight));
            beamType = new StructuralFramingType("Beam", beamProfile, BuiltInMaterials.Concrete);

            //Create mullion types
            Profile mullionProfile = new Profile(Polygon.Rectangle(0.03, 0.1));
            mullionType = new StructuralFramingType("Mullion", mullionProfile, BuiltInMaterials.Steel);

            //Create walls types
            List<MaterialLayer> meetingRoomMaterialLayers = new List<MaterialLayer>(){
                new MaterialLayer(BuiltInMaterials.Glass, 0.1)
            };
            meetingRoomWallType = new WallType("Meeting Room Wall", meetingRoomMaterialLayers);

            List<MaterialLayer> façadeMaterialLayers = new List<MaterialLayer>(){
                new MaterialLayer(BuiltInMaterials.Glass, dim.FacadeDimensions.FacadeThickness)
            };
            facadeType = new WallType("façade", façadeMaterialLayers);

        }

        public FloorType SlabType { get { return slabType; } }
        public FloorType RaisedFloorType { get { return raisedFloorType; } }
        public FloorType CeillingType { get { return ceillingType; } }
        public StructuralFramingType ColumnType { get { return columnType; } }
        public StructuralFramingType BeamType { get { return beamType; } }
        public StructuralFramingType MullionType { get { return mullionType; } }
        public WallType MeetingRoomWallType { get { return meetingRoomWallType; } }
        public WallType FacadeType { get { return facadeType; } }


    }
    public sealed class StructuralDimensions
    {
        private double _beam_height; //full height, including slab thickness
        private double _slab_height;
        private double _column_diameter;
        private double _column_offset;

        private static readonly StructuralDimensions instance = new StructuralDimensions();

        static StructuralDimensions()
        {
        }
        private StructuralDimensions()
        {
            _beam_height = 0.6;
            _slab_height = 0.2;
            _column_diameter = 0.5;
            _column_offset = 0.5;
        }
        public static StructuralDimensions Instance
        {
            get { return instance; }
        }

        public double BeamHeight { get { return _beam_height; } set { _beam_height = value; } }
        public double SlabHeight { get { return _slab_height; } set { _slab_height = value; } }
        public double ColumnDiameter { get { return _column_diameter; } set { _column_diameter = value; } }
        public double ColumnOffset { get { return _column_offset; } set { _column_offset = value; } }
    }

    public sealed class FacadeDimensions
    {
        private double _facade_thickness;
        private double _module_lenght = 1.35;
        private static readonly FacadeDimensions instance = new FacadeDimensions();

        static FacadeDimensions()
        {
        }

        private FacadeDimensions()
        {
            _facade_thickness = 0.4;
            _module_lenght = 1.35;
        }

        public static FacadeDimensions Instance
        {
            get
            {
                return instance;
            }
        }
        public double FacadeThickness { get { return _facade_thickness; } set { _facade_thickness = value; } }
        public double ModuleLenght { get { return _module_lenght; } set { _module_lenght = value; } }
    }

    public sealed class LevelDimensions
    {
        //Set level height definition
        private double _height;
        private double _ceiling_void_height;
        private double _ceiling_thickness;
        private double _raised_floor_void_height;
        private double _raised_floor_thickness;
        private double _headroom;
        private StructuralDimensions _structuralDimensions;
        private static readonly LevelDimensions instance = new LevelDimensions();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static LevelDimensions()
        {
        }

        private LevelDimensions()
        {
            _structuralDimensions = StructuralDimensions.Instance;
            _ceiling_void_height = 0.05;
            _ceiling_thickness = 0.05;
            _raised_floor_void_height = 0.07;
            _raised_floor_thickness = 0.03;
            _headroom = 2.7;

            _height =
_raised_floor_void_height +
_raised_floor_thickness +
_headroom +
_ceiling_thickness +
_ceiling_void_height +
_structuralDimensions.BeamHeight;
        }

        public static LevelDimensions Instance
        {
            get
            {
                return instance;
            }
        }

        public double Height { get { return _height; } set { _height = value; } }
        public double CeilingVoidHeight { get { return _ceiling_void_height; } set { _ceiling_void_height = value; } }
        public double CeilingThickness { get { return _ceiling_thickness; } set { _ceiling_thickness = value; } }
        public double RaisedFloorVoidHeight { get { return _raised_floor_void_height; } set { _raised_floor_void_height = value; } }
        public double RaisedFloorThickness { get { return _raised_floor_thickness; } set { _raised_floor_thickness = value; } }
        public double Headroom { get { return _headroom; } set { 
            _headroom = value; 
                        _height =
_raised_floor_void_height +
_raised_floor_thickness +
_headroom +
_ceiling_thickness +
_ceiling_void_height +
_structuralDimensions.BeamHeight;
            } }
        public StructuralDimensions StructuralDimensions { get { return _structuralDimensions; } }
    }
}