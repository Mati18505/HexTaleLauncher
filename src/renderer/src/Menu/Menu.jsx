import React, { useState, useEffect } from "react";
import SettingsMenu from "./SettingsMenu";
import "./Menu.css";

export default function Menu({menu, onHide}) {
    var [options, SetOptions] = useState();

    useEffect(() => {
        app.launcher.getSettings().then((result)=>{
            SetOptions(JSON.parse(result));
        });
    }, [menu]);

    const availableMenus = ["settings"];

    var visible = availableMenus.includes(menu);

    var classes = "menu ";
    if(!visible)
        classes+="hidden";

    var content = "";

    if(menu == "settings")
    {
        if(options !== undefined)
            content=<SettingsMenu options={options} onNeedHide={onHide}/>;
    }
        

    return <div className={classes}>
        <h1 className="menu-header">{menu}</h1>
        <div className="menu-content">
            {content}
        </div>
    </div>;
}