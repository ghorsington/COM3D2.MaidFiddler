* `ScenarioData.CheckPlayableCondition` => Checks if the scenario is playable
    * Modify return value at the end

* `ScenarioData.AddEventMaid` or `MaidStatus.Status.GetEventFlag` => use to check if maid can attend the event
    * Overwrite to enable maid to reattend the event

* `MaidStatus.Status.eventEndFlags_` => specifies the events that maid has done
    * Use to reset events that maid has completed

* `ScheduleAPI.Visible*` and `ScheduleAPI.Enable*`
    * Use to check if the given work should be shown/enabled for a maid
    * Also `ScheduleWork` constructor