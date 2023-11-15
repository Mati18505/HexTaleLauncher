import React from 'react';
import PlayButton from './PlayButton';
import NavigationMenu from './NavigationMenu';
import './Content.css';
import ProgressBar from './ProgressBar';

export default function Content()
{
    var date = "10.11.2023";
    return <>
    <NavigationMenu/>
        <div className='grid'>
            <div className='contentLeft'>
                <div>
                    <div>
                    <h1 className='title'>HexTale</h1>
                    <p className='description'>Lorem ipsum dolor sit amet consectetur adipisicing elit. Ea nihil, dolores magni quos fugit magnam! Corporis, neque reiciendis ullam, iste, illo molestias quo cupiditate consequuntur nulla aspernatur perspiciatis repellat esse.</p>
                    </div>
                    <div>
                        <ProgressBar label='Downloading...' progress={50}/>
                        <PlayButton enabled={true} />
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