import React, { useState } from "react";
import "./settingsMenuInputs.css"
import "./settingsMenu.css"
import Popup from "../PopupMessage"

function Checkbox({name, state, onChange}) {
    return <label className="container">{name}
        <input type="checkbox" checked={state} onChange={onChange}/>
        <span className="checkmark"></span>
    </label>
}

export function SettingsButton({label, enabled, onClick})
{
    var classes = "settings-button ";
    if(!enabled)
        classes+="settings-button-disabled";

    return <button 
    className={classes}
    type='button' 
    onClick={onClick}>
    <span>{label}</span>
    </button>;
}

function ListButton({label, onClick})
{
    return <div 
    className="settings-list-button"
    onClick={onClick}>
    <span>{label}</span>
    </div>;
}

function SaveBlock({changesToSave, onNeedHide, onSave}) {
    return <div className="save-block">
        <div className="save-buttons-block">
            <SettingsButton label="cancel" enabled={true} onClick={onNeedHide}/>
            <SettingsButton label="save" enabled={changesToSave? true : false} onClick={onSave}/>
        </div>
    </div>;
}

export default function SettingsMenu({options, onNeedHide}) {

    const [exitLauncherWhenGameStarts, SetExitLauncherWhenGameStarts] = useState(options.exitLauncherWhenGameStarts);
    const [changesToSave, SetChangesToSave] = useState(false);

    const ChangeExitLauncherWhenGameStartsChange = () => {
        options.exitLauncherWhenGameStarts = !exitLauncherWhenGameStarts;
        SetExitLauncherWhenGameStarts(!exitLauncherWhenGameStarts);
        SetChangesToSave(true);
    };

    return <div className="settings-container">
        <div>
            <ListButton 
            label={<><i className="icon-folder"></i> Show in folder</>}
            onClick={() => app.launcher.openGamePathInExplorer()}
            />
            <ListButton 
            label={<><i className="icon-wrench"></i> Check and Repair</>}
            onClick={() => app.launcher.repair()}
            />
            <ListButton 
            label={<><i className="icon-trash-empty"></i> Uninstall</>}
            onClick={() => Popup("All data from game folder will be deleted. Uninstall the game?", app.launcher.uninstall)}
            />
            <hr/>
            <Checkbox name="Exit launcher when game starts" state={exitLauncherWhenGameStarts} onChange={ChangeExitLauncherWhenGameStartsChange} />
        </div>
        <div>
            <SaveBlock changesToSave={changesToSave} onNeedHide={onNeedHide} onSave={() => {
                SaveOptions(options);
                SetChangesToSave(false);
            }}/>
        </div>
    </div>;
}

function SaveOptions(options) {
    app.launcher.saveSettings(options);
}