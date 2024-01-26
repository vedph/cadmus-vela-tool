using Cadmus.Core.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Cadmus.Vela.Import;

public class ThesaurusEntryMap
{
    private Dictionary<string, Thesaurus> _thesauri;
    private Dictionary<string, string> _aliases;

    public ThesaurusEntryMap()
    {
        _thesauri = [];
        _aliases = [];
    }

    public void Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        Thesaurus[] thesauri = JsonSerializer.Deserialize<Thesaurus[]>(stream)!;

        _thesauri = thesauri.Where(t => t.TargetId == null)
            .ToDictionary(t => t.Id);
        _aliases = thesauri.Where(t => t.TargetId != null)
            .ToDictionary(t => t.Id, t => t.TargetId!);
    }

    public Thesaurus? GetThesaurus(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return _thesauri.TryGetValue(id, out Thesaurus? thesaurus)
            ? thesaurus
            : null;
    }

    public string? GetEntryId(string thesaurusId, string entryValue)
    {
        ArgumentNullException.ThrowIfNull(thesaurusId);
        ArgumentNullException.ThrowIfNull(entryValue);

        if (_aliases.TryGetValue(thesaurusId, out string? alias))
            thesaurusId = alias;

        if (!_thesauri.TryGetValue(thesaurusId, out Thesaurus? thesaurus))
            return null;

        return thesaurus.Entries.FirstOrDefault(e => e.Value == entryValue)?.Id;
    }
}
