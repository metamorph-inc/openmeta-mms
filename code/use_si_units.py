gme.MgaProject.BeginTransactionInNewTerr()

units = gme.MgaProject.RootFolder.GetObjectByPathDisp('/@UnitLibrary QUDT/@TypeSpecifications/@Units')

si_units = dict(((unit.Name, unit) for unit in units.ChildObjects if unit.MetaBase.Name == 'si_unit'))

for unit in units.ChildObjects:
    if unit.Meta.Name == 'si_unit': continue
    if not unit.Name in si_units: continue
    for ref in unit.ReferencedBy:
        if ref.IsLibObject: continue
        ref.Referred = si_units[unit.Name]
    

gme.ConsoleMessage(repr(si_units.keys()), 1)

gme.MgaProject.CommitTransaction()
