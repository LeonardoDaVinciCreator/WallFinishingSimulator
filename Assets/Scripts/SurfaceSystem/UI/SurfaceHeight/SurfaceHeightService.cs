namespace SurfaceSystem.UI.SurfaceHeight
{
    public class SurfaceHeightService
    {
        private float _currentOffset = 0f;
        private float _bottomOffset = 0f;
        private float _topOffset = 0f;

        public float CurrentOffset => _currentOffset;
        public float BottomOffset => _bottomOffset;
        public float TopOffset => _topOffset;

        public void SetCurrentOffset(float value)
        {
            _currentOffset = value;
        }

        public void AddTopOffset(float delta)
        {
            _topOffset += delta;
        }

        public void AddBottomOffset(float delta)
        {
            _bottomOffset += delta;
        }

        public void ResetOffsets()
        {
            _currentOffset = 0f;
            _bottomOffset = 0f;
            _topOffset = 0f;
        }
    }
}