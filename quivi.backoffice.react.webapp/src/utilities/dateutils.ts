export class DateUtils {
    static toString = (date: string | Date) => {
        const d = typeof date === 'string' ? this.toDate(date) : date;
        const year = d.getFullYear();
        const month = d.getMonth() + 1;
        const day = d.getDate();
        const hour = d.getHours();
        const minute = d.getMinutes();
        const format = (n: number) => n.toLocaleString(undefined, {minimumIntegerDigits: 2});
        
        const result = `${year}-${format(month)}-${format(day)} ${format(hour)}:${format(minute)}`;
        return result;
    }
    
    static toDate = (date: string | Date): Date => {
        if (typeof date === 'string') {
            return new Date(Date.parse(date));
        }
        return date;
    }
}