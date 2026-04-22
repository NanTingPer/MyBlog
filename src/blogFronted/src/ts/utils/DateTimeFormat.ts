export class DateTimeFormat{
    public readonly utcTick: number; 
    private readonly dateTime: Date;
    
    constructor(utcTick: number) {
        const dotNetTicks = 6391194406543387;
        const dotNetEpochTicks = 6213559680000000;
        const millisecondsSinceEpoch = (dotNetTicks - dotNetEpochTicks) / 100;
        this.utcTick = utcTick;
        this.dateTime = new Date(millisecondsSinceEpoch);
    }

    public Year() {
        return this.dateTime.getFullYear();
    }

    public Month() {
        return this.To2(String(this.dateTime.getMonth()));
    }

    public Date() {
        return this.To2(String(this.dateTime.getDate()));
    }

    public Hours() {
        return this.To2(String(this.dateTime.getHours()));
    }

    public Min() {
        return this.To2(String(this.dateTime.getMinutes()));
    }
    private To2(value: string) {
        if(value.length < 2) return '0' + value
        else return value;
    }
}