import React, { useState, useEffect } from 'react';
import PlayButton from './PlayButton';
import './Content.css';
import ProgressBar from './ProgressBar';
import InfoText from './InfoText';

export default function Content()
{
    const date = "10.11.2023";
    const [progressBarValue, SetProgress] = useState(0);
    const [info, SetInfo] = useState("");
    const [PBVisible, SetPBVisible] = useState(false);
    const [status, SetStatus] = useState("Play");
    const [playEnabled, SetPlayEnabled] = useState(true);

    useEffect(()=>{
        app.launcher.progressBarValue((event, value) => SetProgress(value) );
        app.launcher.info((event, message) => SetInfo(message) );
        app.launcher.progressBarVisible((event, visible) => SetPBVisible(visible) );
        app.launcher.status((event, message) => {
            SetStatus(message);
            SetPlayEnabled(message == "Play");
        } );
    }, []);

    return <>
        <div className='grid'>
            <div className='contentLeft'>
                <div>
                    <div>
                    <h1 className='title'>HexTale</h1>
                    <p className='description'>Lorem ipsum dolor sit amet consectetur adipisicing elit. Ea nihil, dolores magni quos fugit magnam! Corporis, neque reiciendis ullam, iste, illo molestias quo cupiditate consequuntur nulla aspernatur perspiciatis repellat esse.</p>
                    </div>
                    <div>
                        <InfoText text={info} />
                        {PBVisible && <ProgressBar progress={progressBarValue}/>}
                        <PlayButton enabled={playEnabled} label={status} />
                    </div>
                </div>
            </div>
            <div className='contentRight'>
                <div>
                    <div className='box'>
                        <h1><i className='icon-star'></i> Status</h1>
                        <ul>
                            <li><i className='icon-lightbulb'></i> Server Online</li>
                            <li><i className='icon-users'></i> 299 players</li>
                        </ul>
                    </div>
                    <div className='box'>
                        <h1><i className='icon-calendar'></i> News & Events</h1>
                        <ul>
                            <li><span className='date'>{date}</span> Changed lorem ipsum</li>
                            <li><span className='date'>{date}</span> Added lorem ipsum</li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    </>;
}