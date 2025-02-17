% instantiate the library
disp('Loading the library...');
lib = lsl_loadlib();

% resolve a stream...
disp('Resolving an EEG stream...');
result = {};
while isempty(result)
    result = lsl_resolve_byprop(lib,'type','EEG'); end

% create a new inlet
disp('Opening an inlet...');
inlet = lsl_inlet(result{1});

disp('Now receiving chunked data...');
for i=1:10
%while true
    % get chunk from the inlet
    [chunk,stamps] = inlet.pull_chunk();
    siz=size(chunk);
    sizch(i)=siz(2);
    %pause(0.5)
    for s=1:length(stamps)
        % and display it
        fprintf('%.2f\t',chunk(:,s));
        fprintf('%.5f\n',stamps(s));
    end
    i
    pause(1);
end