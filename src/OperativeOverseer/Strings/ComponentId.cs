using System;

namespace OperativeOverseer.Strings;

public class ComponentId
{
    private readonly int _project;
    private readonly short _feature;
    private readonly byte _part;
    private byte _component = 0;

    public ComponentId(int projectId, byte featureId, byte part)
    {
        _project = projectId;
        _feature = featureId;
        _part = part;
    }

    public string Next()
    {
        var partAndComponent = (short)(((uint)_part) << 8 | (uint)(_component++));
        var id = new Guid(_project, _feature, partAndComponent, new byte[8]);
        return id.ToString("n");
    }

    public static implicit operator ComponentId((int project, int feature) value)
    {
        return new ComponentId(value.project, (byte)value.feature, 0);
    }
}
