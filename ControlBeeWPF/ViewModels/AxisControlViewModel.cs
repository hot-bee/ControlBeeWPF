using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ControlBeeWPF.ViewModels;

public partial class AxisControlViewModel : ObservableObject
{
    [ObservableProperty]
    private AxisInfo? _selectedAxis;

    public string PositionUnit { get; set; } = "";
    public string VelocityUnit { get; set; } = "";
    public string AccelDecelUnit { get; set; } = "";
    public string JerkUnit { get; set; } = "";

    public ObservableCollection<AxisInfo> Axes { get; } = [];

    public sealed partial class AxisInfo : ObservableObject
    {
        public int Index { get; }
        public AxisStatusViewModel AxisStatusViewModel { get; }
        public VariableViewModel? JogStepSmallViewModel { get; }
        public VariableViewModel? JogStepMediumViewModel { get; }
        public VariableViewModel? JogStepLargeViewModel { get; }
        public VariableViewModel? JogSpeedSlowViewModel { get; }
        public VariableViewModel? JogSpeedMediumViewModel { get; }
        public VariableViewModel? JogSpeedFastViewModel { get; }

        public string ActorName { get; }
        public string ItemPath { get; }
        public VariableViewModel? InitVelocityViewModel { get; }
        public VariableViewModel? InitAccelViewModel { get; }
        public VariableViewModel? InitDecelViewModel { get; }
        public VariableViewModel? InitAccelJerkViewModel { get; }
        public VariableViewModel? InitDecelJerkViewModel { get; }
        public VariableViewModel? JogVelocityViewModel { get; }
        public VariableViewModel? JogAccelViewModel { get; }
        public VariableViewModel? JogDecelViewModel { get; }
        public VariableViewModel? JogAccelJerkViewModel { get; }
        public VariableViewModel? JogDecelJerkViewModel { get; }
        public VariableViewModel? NormalVelocityViewModel { get; }
        public VariableViewModel? NormalAccelViewModel { get; }
        public VariableViewModel? NormalDecelViewModel { get; }
        public VariableViewModel? NormalAccelJerkViewModel { get; }
        public VariableViewModel? NormalDecelJerkViewModel { get; }

        [ObservableProperty]
        private string _axisName;

        [ObservableProperty]
        private double _commandPos;

        [ObservableProperty]
        private double _actualPos;

        [ObservableProperty]
        private bool _isNegativePosition;

        [ObservableProperty]
        private bool _isPositivePosition;

        [ObservableProperty]
        private bool _isHomeDet;

        public AxisInfo(
            int index,
            AxisStatusViewModel axisStatusViewModel,
            string actorName,
            string itemPath,
            VariableViewModel? jogStepSmallViewModel = null,
            VariableViewModel? jogStepMediumViewModel = null,
            VariableViewModel? jogStepLargeViewModel = null,
            VariableViewModel? jogSpeedSlowViewModel = null,
            VariableViewModel? jogSpeedMediumViewModel = null,
            VariableViewModel? jogSpeedFastViewModel = null,
            VariableViewModel? initVelocityViewModel = null,
            VariableViewModel? initAccelViewModel = null,
            VariableViewModel? initDecelViewModel = null,
            VariableViewModel? initAccelJerkViewModel = null,
            VariableViewModel? initDecelJerkViewModel = null,
            VariableViewModel? jogVelocityViewModel = null,
            VariableViewModel? jogAccelViewModel = null,
            VariableViewModel? jogDecelViewModel = null,
            VariableViewModel? jogAccelJerkViewModel = null,
            VariableViewModel? jogDecelJerkViewModel = null,
            VariableViewModel? normalVelocityViewModel = null,
            VariableViewModel? normalAccelViewModel = null,
            VariableViewModel? normalDecelViewModel = null,
            VariableViewModel? normalAccelJerkViewModel = null,
            VariableViewModel? normalDecelJerkViewModel = null
        )
        {
            Index = index;
            AxisStatusViewModel = axisStatusViewModel;

            ActorName = actorName;
            ItemPath = itemPath;

            JogStepSmallViewModel = jogStepSmallViewModel;
            JogStepMediumViewModel = jogStepMediumViewModel;
            JogStepLargeViewModel = jogStepLargeViewModel;

            JogSpeedSlowViewModel = jogSpeedSlowViewModel;
            JogSpeedMediumViewModel = jogSpeedMediumViewModel;
            JogSpeedFastViewModel = jogSpeedFastViewModel;

            InitVelocityViewModel = initVelocityViewModel;
            InitAccelViewModel = initAccelViewModel;
            InitDecelViewModel = initDecelViewModel;
            InitAccelJerkViewModel = initAccelJerkViewModel;
            InitDecelJerkViewModel = initDecelJerkViewModel;

            JogVelocityViewModel = jogVelocityViewModel;
            JogAccelViewModel = jogAccelViewModel;
            JogDecelViewModel = jogDecelViewModel;
            JogAccelJerkViewModel = jogAccelJerkViewModel;
            JogDecelJerkViewModel = jogDecelJerkViewModel;

            NormalVelocityViewModel = normalVelocityViewModel;
            NormalAccelViewModel = normalAccelViewModel;
            NormalDecelViewModel = normalDecelViewModel;
            NormalAccelJerkViewModel = normalAccelJerkViewModel;
            NormalDecelJerkViewModel = normalDecelJerkViewModel;

            _axisName = $"/{actorName}{itemPath}";
            _commandPos = axisStatusViewModel.CommandPosition;
            _actualPos = axisStatusViewModel.ActualPosition;
            _isNegativePosition = axisStatusViewModel.IsNegativeLimitDet;
            _isPositivePosition = axisStatusViewModel.IsPositiveLimitDet;
            _isHomeDet = axisStatusViewModel.IsHomeDet;

            AxisStatusViewModel.PropertyChanged += AxisStatusViewModelOnPropertyChanged;
            RegisterSync(InitAccelViewModel, InitDecelViewModel);
            RegisterSync(InitAccelJerkViewModel, InitDecelJerkViewModel);
            RegisterSync(JogAccelViewModel, JogDecelViewModel);
            RegisterSync(JogAccelJerkViewModel, JogDecelJerkViewModel);
            RegisterSync(NormalAccelViewModel, NormalDecelViewModel);
            RegisterSync(NormalAccelJerkViewModel, NormalDecelJerkViewModel);
        }

        private static void RegisterSync(VariableViewModel? source, VariableViewModel? target)
        {
            if (source == null || target == null)
                return;

            source.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(VariableViewModel.Value))
                    return;

                if (target.Value is null)
                    return;

                target.ChangeValue($"{source.Value}");
            };
        }

        private void AxisStatusViewModelOnPropertyChanged(
            object? sender,
            PropertyChangedEventArgs e
        )
        {
            switch (e.PropertyName)
            {
                case nameof(AxisStatusViewModel.CommandPosition):
                    CommandPos = AxisStatusViewModel.CommandPosition;
                    break;
                case nameof(AxisStatusViewModel.ActualPosition):
                    ActualPos = AxisStatusViewModel.ActualPosition;
                    break;
                case nameof(AxisStatusViewModel.IsNegativeLimitDet):
                    IsNegativePosition = AxisStatusViewModel.IsNegativeLimitDet;
                    break;
                case nameof(AxisStatusViewModel.IsPositiveLimitDet):
                    IsPositivePosition = AxisStatusViewModel.IsPositiveLimitDet;
                    break;
                case nameof(AxisStatusViewModel.IsHomeDet):
                    IsHomeDet = AxisStatusViewModel.IsHomeDet;
                    break;
            }
        }
    }
}
